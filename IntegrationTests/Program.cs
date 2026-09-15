using System.Diagnostics;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json.Nodes;
using Desktop_Pacienskezelo;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal static class Program
{
    private static int passed;
    [STAThread]
    private static int Main(string[] args)
    {
        var root = Path.GetFullPath(args.FirstOrDefault() ?? Environment.CurrentDirectory);
        try {
            if (args.Contains("--model-only"))
            {
                using var modelDb = new SqlServerDbContext(new DbContextOptionsBuilder<SqlServerDbContext>()
                    .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MedActivities_ModelOnly;Integrated Security=True").Options);
                string[] triggerTables = ["Patients", "Practitioners", "Activities", "Appointments", "PatientActivities",
                    "ActivityPractitioners", "PatientPractitionerAccesses", "PractitionerBookingSettings",
                    "PractitionerWorkingHours", "PatientNotes", "PatientDocuments", "AdminProfiles",
                    "AdmissionsOfficeProfiles", "ActivityComments", "DeletedRecords"];
                foreach (var table in triggerTables)
                    Check(!modelDb.Model.GetEntityTypes().Single(e => e.GetTableName() == table).IsSqlOutputClauseUsed(),
                        table + " uses trigger-compatible SQL commands");
                Console.WriteLine($"SUCCESS: {passed} model checks passed (no database connection).");
                return 0;
            }
            var practitionerOnly = args.Contains("--practitioner");
            VerifyApi(root, practitionerOnly).GetAwaiter().GetResult();
            if (!practitionerOnly) VerifyForms(root);
            Console.WriteLine($"SUCCESS: {passed} checks passed.");
            return 0;
        } catch (Exception ex) { Console.Error.WriteLine("FAIL: " + ex); return 1; }
    }
    private static void Check(bool condition, string name)
    { if (!condition) throw new InvalidOperationException(name); passed++; Console.WriteLine("PASS: " + name); }
    private static async Task VerifyApi(string root, bool practitionerOnly = false)
    {
        // Mindig új, egyértelműen teszt nevű adatbázis. Nem fogad el üzemi connection stringet.
        var dbName = "MedActivities_Test_" + Guid.NewGuid().ToString("N");
        var connection = $"Server=(localdb)\\MSSQLLocalDB;Database={dbName};Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        var options = new DbContextOptionsBuilder<SqlServerDbContext>().UseSqlServer(connection).Options;
        var secret = "Test!A9-" + Guid.NewGuid().ToString("N");
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        using var process = new Process {
            StartInfo = new ProcessStartInfo("dotnet") {
                WorkingDirectory = Path.Combine(root, "API"), UseShellExecute = false, CreateNoWindow = true,
                RedirectStandardOutput = true, RedirectStandardError = true
            }
        };
        var start = process.StartInfo;
        start.ArgumentList.Add(Path.Combine(root, "API", "bin", "Debug", "net10.0", "API.dll"));
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Production";
        start.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        start.Environment["Database__Provider"] = "SqlServer";
        start.Environment["Database__ApplyMigrations"] = "true";
        start.Environment["Database__SeedDemoData"] = "false";
        start.Environment["ConnectionStrings__SqlServerConnection"] = connection;
        start.Environment["BootstrapAdmin__Email"] = "admin@test.invalid";
        start.Environment["BootstrapAdmin__Password"] = secret;
        var logs = new StringBuilder();
        process.OutputDataReceived += (_, e) => { lock(logs) logs.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { lock(logs) logs.AppendLine(e.Data); };
        try {
            process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
            using var anonymous = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}/api/"), Timeout = TimeSpan.FromSeconds(10) };
            bool ready = false;
            for (int i = 0; i < 100 && !process.HasExited; i++) {
                try { using var r = await anonymous.GetAsync("health"); if (r.IsSuccessStatusCode) { ready = true; break; } } catch (HttpRequestException) { }
                await Task.Delay(300);
            }
            if (!ready) throw new InvalidOperationException("API startup failed: " + logs);
            Check((await anonymous.GetFromJsonAsync<JsonObject>("health"))!["database"]!.GetValue<string>() == "SqlServer", "SQL Server health and real migrations");
            await Expect(anonymous, "GET", "patients", null, 401);
            await Expect(anonymous, "GET", "missing-endpoint", null, 404);
            await Expect(anonymous, "POST", "session/login", new { userName = "admin@test.invalid", password = "incorrect" }, 401);
            using var admin = await Login(anonymous.BaseAddress!, "admin@test.invalid", secret);
            using var desktop = new ApiClient();
            await desktop.Login(anonymous.BaseAddress!.ToString(), "admin@test.invalid", secret);
            Check(desktop.Staff && desktop.Session is not null, "WinForms ApiClient authenticates against SQL Server-backed API");

            object PatientInput(string name, string taj) => new { name, tajNumber = taj, birthDate = "1990-02-03", email = (string?)null, phone = "123", address = "Budapest", notes = "Teszt" };
            var p1 = (await Expect(admin, "POST", "patients", PatientInput("Teszt Első", "012345678"), 201))!["id"]!.GetValue<string>();
            var p2 = (await Expect(admin, "POST", "patients", PatientInput("Teszt Második", "012345679"), 201))!["id"]!.GetValue<string>();
            Check((await desktop.Get<List<Patient>>("patients")).Count == 2, "Shared data and multiple nullable UserIds");
            await Expect(admin, "POST", "patients", PatientInput("Duplikált", "012345678"), 409);
            await Expect(admin, "POST", "patients", PatientInput("Hibás TAJ", "abcdefgh9"), 400);
            await Expect(admin, "PUT", "patients/" + p1, PatientInput("Módosított", "012345678"), 204);
            Check((await desktop.Get<Patient>("patients/" + p1)).Name == "Módosított", "Patient update visible from WinForms client");

            await Expect(anonymous, "POST", "session/register", new { userName = "doctor", email = "doctor@test.invalid", password = secret, name = "Teszt Orvos", role = "Practitioner", tajNumber = "900000001", birthDate = "1980-01-01" }, 201);
            await Expect(anonymous, "POST", "session/register", new { userName = "patient", email = "patient@test.invalid", password = secret, name = "Saját Páciens", role = "Patient", tajNumber = "800000001", birthDate = "1990-01-01" }, 201);
            using var doctor = await Login(anonymous.BaseAddress, "doctor", secret);
            using var patient = await Login(anonymous.BaseAddress, "patient", secret);
            await Expect(admin, "POST", "user-management", new { userName = "office", email = "office@test.invalid", password = secret, name = "Felvételi teszt", role = "AdmissionsOffice" }, 201);
            using var office = await Login(anonymous.BaseAddress, "office", secret);
            var doctors = (await Expect(admin, "GET", "practitioners", null, 200))!.AsArray();
            var doctorId = doctors[0]!["id"]!.GetValue<string>();
            var doctorUserId = (await Expect(doctor, "GET", "session/me", null, 200))!["id"]!.GetValue<string>();
            await Expect(admin, "POST", "practitioners", new { name = "Dupla profil", tajNumber = "900000002", userId = doctorUserId, specialty = "Orvos", city = "Budapest", venue = "Rendelő" }, 409);
            Check((await Expect(admin, "GET", "practitioners/available-accounts", null, 200))!.AsArray().Count == 0, "Linked practitioner account is not offered again");
            await Expect(doctor, "DELETE", "practitioners/" + doctorId, null, 403);
            await Expect(office, "DELETE", "practitioners/" + doctorId, null, 403);
            await Expect(patient, "DELETE", "practitioners/" + doctorId, null, 403);
            await Expect(doctor, "PUT", $"practitioners/{doctorId}/working-hours", new { dayOfWeek = 1, isWorkingDay = true, startTime = "08:00:00", endTime = "16:00:00" }, 403);
            await Expect(doctor, "GET", "patients/" + p1, null, 404);
            await Expect(patient, "GET", "patients/" + p1, null, 404);
            await Expect(patient, "POST", "patients", PatientInput("Tiltott", "123123123"), 403);
            Check((await Expect(doctor, "GET", "patients", null, 200))!.AsArray().Count == 0, "Unassigned patients excluded from health-record choices");
            Check((await Expect(doctor, "GET", "patients/directory", null, 200))!["all"]!.AsArray().Any(p => p!["id"]!.GetValue<string>() == p1), "Practitioner can read unassigned patient directory");
            await Expect(doctor, "POST", "appointments", new { patientId = p1, practitionerId = doctorId, date = DateOnly.FromDateTime(DateTime.Today.AddDays(3)), hour = 10 }, 403);
            await Expect(doctor, "PUT", $"patients/{p1}/access/{doctorId}", new {}, 204);
            await Expect(doctor, "PUT", $"patients/{p1}/access/{doctorId}", new {}, 204);
            Check((await Expect(doctor, "GET", "patients", null, 200))!.AsArray().Count == 1, "Self assignment is immediate and idempotent");
            await Expect(doctor, "GET", "patients/" + p1, null, 200);
            await Expect(admin, "PUT", $"practitioners/{doctorId}/booking-enabled", new { bookingEnabled = true }, 204);
            var day = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            await Expect(admin, "PUT", $"practitioners/{doctorId}/working-hours", new { dayOfWeek = (int)day.DayOfWeek, isWorkingDay = true, startTime = "08:00:00", endTime = "20:00:00" }, 204);

            object ActivityInput(string? id, string title) => new { id, title, date = day.ToDateTime(new TimeOnly(10,0)), description = "Leírás", category = "Vizsgálat", city = "Budapest", venue = "Rendelő", patientId = p1, practitionerIds = new[] { doctorId }, status = "Scheduled" };
            var activityId = (await Expect(admin, "POST", "activities", ActivityInput(null, "Esemény"), 201))!.GetValue<string>();
            await Expect(doctor, "GET", "activities/" + activityId, null, 200);
            Check((await Expect(admin, "GET", $"activities?patientId={p1}&practitionerId={doctorId}", null, 200))!.AsArray().Count == 1, "MediatR patient and practitioner filters combine");
            Check((await Expect(doctor, "GET", $"activities?patientId={p2}", null, 200))!.AsArray().Count == 0, "Filter cannot broaden practitioner access");
            await Expect(anonymous, "POST", "session/register", new { userName = "otherdoctor", email = "otherdoctor@test.invalid", password = secret, name = "Másik Orvos", role = "Practitioner", tajNumber = "900000003", birthDate = "1980-01-01" }, 201);
            var otherDoctorId = (await Expect(admin, "GET", "practitioners", null, 200))!.AsArray().Single(p => p!["id"]!.GetValue<string>() != doctorId)!["id"]!.GetValue<string>();
            object HistoryInput(string? id) => new { id, title = "Másik orvos eseménye", date = day.ToDateTime(new TimeOnly(9, 0)), description = "Előzmény", category = "Vizsgálat", city = "Budapest", venue = "Rendelő", patientId = p1, practitionerIds = new[] { otherDoctorId } };
            var historyId = (await Expect(admin, "POST", "activities", HistoryInput(null), 201))!.GetValue<string>();
            Check(!(await Expect(doctor, "GET", "activities/" + historyId, null, 200))!["canEditFields"]!.GetValue<bool>(), "Assigned patient's other-doctor event is read only");
            Check((await Expect(doctor, "GET", $"activities?patientId={p1}", null, 200))!.AsArray().Count == 2, "Assigned patient's full event history is listed");
            await Expect(doctor, "PUT", "activities", HistoryInput(historyId), 403);
            if (practitionerOnly)
            {
                await Expect(doctor, "POST", "appointments", new { patientId = p2, practitionerId = doctorId, date = day, hour = 10 }, 403);
                await Expect(doctor, "POST", "appointments", new { patientId = p1, practitionerId = doctorId, date = day, hour = 10 }, 201);
                await Expect(doctor, "POST", "appointments", new { patientId = p1, practitionerId = doctorId, date = day, hour = 11 }, 409);
                await Expect(admin, "DELETE", $"patients/{p1}/access/{doctorId}", null, 204);
                await Expect(doctor, "GET", "activities/" + historyId, null, 404);
                Check((await Expect(doctor, "GET", "patients", null, 200))!.AsArray().Count == 0, "Revocation removes health-record access immediately");
                return;
            }
            await Expect(admin, "DELETE", "activities/" + historyId, null, 204);
            await Expect(admin, "DELETE", "practitioners/" + otherDoctorId, null, 204);
            await VerifyChat(anonymous, admin, doctor, patient, activityId);
            await Expect(admin, "PUT", "activities", ActivityInput(activityId, "Módosított esemény"), 204);
            await Expect(admin, "DELETE", "patients/" + p1, null, 409);
            await Expect(admin, "DELETE", "activities/" + activityId, null, 204);

            var records = $"patients/{p1}/records";
            var note = (await Expect(doctor, "POST", records + "/notes", new { text = "Orvosi megjegyzés" }, 201))!["id"]!.GetValue<string>();
            await Expect(patient, "GET", records + "/notes", null, 404);
            await Expect(doctor, "PUT", records + "/notes/" + note, new { text = "Módosított megjegyzés" }, 204);
            Check((await desktop.Get<List<NoteItem>>(records + "/notes")).Single().Text.StartsWith("Módosított"), "SQL note CRUD shared between clients");
            await Expect(admin, "DELETE", "patients/" + p1, null, 409);
            await Expect(admin, "DELETE", records + "/notes/" + note, null, 204);
            var bytes = Encoding.UTF8.GetBytes("Teszt dokumentum – UTF-8");
            using var upload = new MultipartFormDataContent();
            upload.Add(new StringContent("Teszt dokumentum"), "title"); upload.Add(new ByteArrayContent(bytes), "file", "teszt.txt");
            using var uploaded = await doctor.PostAsync(records + "/documents", upload);
            Check((int)uploaded.StatusCode == 201, "Document upload to SQL varbinary(max)");
            var docId = (await uploaded.Content.ReadFromJsonAsync<JsonObject>())!["id"]!.GetValue<string>();
            Check((await desktop.Get<List<DocumentItem>>(records + "/documents")).Single().Size == bytes.Length, "Document metadata and size");
            Check((await desktop.Download(records + "/documents/" + docId + "/content")).SequenceEqual(bytes), "Binary document roundtrip");
            await Expect(patient, "GET", records + "/documents/" + docId + "/content", null, 404);
            await Expect(admin, "PUT", records + "/documents/" + docId, new { title = "Átnevezett" }, 204);
            using var badUpload = new MultipartFormDataContent();
            badUpload.Add(new StringContent("Hamis PDF"), "title"); badUpload.Add(new ByteArrayContent(bytes), "file", "fake.pdf");
            using var bad = await admin.PostAsync(records + "/documents", badUpload);
            Check((int)bad.StatusCode == 400, "Wrong document signature rejected");
            await Expect(admin, "DELETE", records + "/documents/" + docId, null, 204);

            var booking = (await Expect(doctor, "POST", "appointments", new { patientId = p1, practitionerId = doctorId, date = day, hour = 10, note = "Megmarad" }, 201))!;
            var bookingId = booking["id"]!.GetValue<string>();
            await Expect(admin, "POST", "appointments", new { patientId = p2, practitionerId = doctorId, date = day, hour = 10 }, 409);
            await Expect(admin, "POST", "appointments", new { patientId = p1, practitionerId = doctorId, date = day, hour = 11 }, 409);
            await Expect(admin, "PUT", "appointments/" + bookingId, new { date = day, hour = 12 }, 204);
            var moved = await desktop.Get<AppointmentItem>("appointments/" + bookingId);
            Check(moved.StartTime.Hour == 12 && moved.Note == "Megmarad", "Reschedule synchronizes and preserves note");
            var activity = await desktop.Get<ActivityItem>("activities/" + moved.ActivityId);
            Check(activity.Date.Hour == 12, "Appointment and activity dates match");
            await Expect(doctor, "PUT", $"appointments/{bookingId}/status", new { status = "Completed" }, 409);
            await Expect(patient, "GET", "appointments/" + bookingId, null, 404);
            await Expect(admin, "POST", $"appointments/{bookingId}/cancel", new {}, 204);
            Check((await admin.GetFromJsonAsync<int[]>($"appointments/slots?practitionerId={doctorId}&date={day:yyyy-MM-dd}"))!.Contains(12), "Cancellation frees slot");
            await Expect(admin, "DELETE", "appointments/" + bookingId, null, 204);
            await Expect(admin, "GET", "activities/" + moved.ActivityId, null, 404);
            await Expect(admin, "DELETE", "patients/" + p1, null, 204);
            await Expect(admin, "DELETE", "patients/" + p2, null, 204);
            await Expect(admin, "DELETE", "practitioners/" + doctorId, null, 204);
            await using var db = new SqlServerDbContext(options);
            await db.Database.MigrateAsync();
            Check(!(await db.Database.GetPendingMigrationsAsync()).Any(), "SQL migrations are repeatable, no pending changes");
            var archivedTables = await db.DeletedRecords.Select(r => r.TableName).Distinct().ToListAsync();
            Check(new[] { "Patients", "Practitioners", "Activities", "Appointments", "PatientNotes", "PatientDocuments", "PatientActivities", "ActivityPractitioners", "ActivityComments", "PractitionerWorkingHours", "PractitionerBookingSettings" }.All(archivedTables.Contains), "SQL triggers archive main records and cascaded children");
            var archivedDocument = await db.DeletedRecords.SingleAsync(r => r.TableName == "PatientDocuments");
            Check(Convert.FromBase64String(JsonNode.Parse(archivedDocument.SnapshotJson)!["Content"]!.GetValue<string>()).SequenceEqual(bytes), "Deleted document archive contains the full binary content");
            Check(await db.DeletedRecords.Where(r => r.TableName == "Patients").AllAsync(r => r.DeletedBy != null && r.DeletedAtUtc != default), "Deletion archive records actor and UTC time");
            try { await db.Database.ExecuteSqlRawAsync("DELETE FROM [DeletedRecords]"); throw new InvalidOperationException("Archive deletion was permitted."); }
            catch (SqlException ex) when (ex.Number == 51001) { Check(true, "Deletion archive is protected from DELETE"); }
            await Expect(patient, "POST", "session/logout", new { }, 204);
            await Expect(patient, "GET", "session/me", null, 401);
            using var reloggedPatient = await Login(anonymous.BaseAddress, "patient", secret);
            await Expect(reloggedPatient, "GET", "session/me", null, 200);
            await VerifyDemo(process, anonymous, db);
        }
        finally {
            if (process.Id != 0 && !process.HasExited) { process.Kill(true); await process.WaitForExitAsync(); }
            SqlConnection.ClearAllPools();
            // Csak az e futás által generált adatbázis törölhető.
            if (!dbName.StartsWith("MedActivities_Test_", StringComparison.Ordinal) || dbName.Length != 51) throw new InvalidOperationException("Unsafe test cleanup.");
            await using var cleanup = new SqlServerDbContext(options);
            await cleanup.Database.EnsureDeletedAsync();
        }
    }

    private static async Task VerifyChat(HttpClient anonymous, HttpClient admin, HttpClient doctor, HttpClient patient, string activityId)
    {
        await Expect(anonymous, "POST", "chat/negotiate?negotiateVersion=1", null, 401);
        async Task<ClientWebSocket> Connect(HttpClient user) {
            var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", user.DefaultRequestHeaders.Authorization!.ToString());
            var uri = new UriBuilder(new Uri(user.BaseAddress!, "chat")) { Scheme = "ws", Query = "activityId=" + activityId };
            await socket.ConnectAsync(uri.Uri, CancellationToken.None);
            await Send(socket, "{\"protocol\":\"json\",\"version\":1}");
            await Receive(socket);
            return socket;
        }
        async Task Send(ClientWebSocket socket, string message) => await socket.SendAsync(Encoding.UTF8.GetBytes(message + "\u001e"), WebSocketMessageType.Text, true, CancellationToken.None);
        async Task<string> Receive(ClientWebSocket socket) {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var buffer = new byte[65536]; var message = new StringBuilder();
            WebSocketReceiveResult result;
            do { result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), timeout.Token); message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count)); } while (!result.EndOfMessage);
            return message.ToString();
        }
        async Task<JsonNode> Invoke(ClientWebSocket socket, string id, string target, params object[] arguments) {
            await Send(socket, System.Text.Json.JsonSerializer.Serialize(new { type = 1, invocationId = id, target, arguments }));
            for (var i = 0; i < 10; i++) foreach (var message in (await Receive(socket)).Split('\u001e', StringSplitOptions.RemoveEmptyEntries)) {
                var json = JsonNode.Parse(message)!;
                if (json["invocationId"]?.GetValue<string>() == id) return json;
                if (json["type"]?.GetValue<int>() == 7) throw new InvalidOperationException("Chat connection closed.");
            }
            throw new InvalidOperationException("Chat response missing.");
        }
        using var sender = await Connect(admin);
        using var receiver = await Connect(doctor);
        var blank = await Invoke(sender, "blank", "SendComment", "   ");
        Check(blank["error"] is not null, "Chat rejects empty messages");
        var sent = await Invoke(sender, "send", "SendComment", "SQL Server chat teszt <script>nem HTML</script>");
        Check(sent["result"]?["body"]?.GetValue<string>().StartsWith("SQL Server chat teszt") == true, "SignalR message saved through MediatR");
        var history = await Invoke(receiver, "history", "LoadComments");
        Check(history["result"]!.AsArray().Count == 1, "Second authorized connection reads persisted chat");
        // A patient without a link to this activity cannot join/read the group.
        using var denied = await Connect(patient);
        var deniedResult = false;
        try { var response = await Invoke(denied, "denied", "LoadComments"); deniedResult = response["error"] is not null; }
        catch (Exception ex) when (ex is WebSocketException or InvalidOperationException) { deniedResult = true; }
        Check(deniedResult, "Unrelated patient cannot read event chat");
        await sender.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
        await receiver.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
    }

    private static async Task VerifyDemo(Process server, HttpClient anonymous, SqlServerDbContext db)
    {
        const string prefix = "egeszsegut-demo-v1-";
        await Expect(anonymous, "POST", "dev-session/Admin", new { }, 404);
        async Task Restart(string environment) {
            if (!server.HasExited) { server.Kill(true); await server.WaitForExitAsync(); }
            server.CancelOutputRead(); server.CancelErrorRead();
            server.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = environment;
            server.Start(); server.BeginOutputReadLine(); server.BeginErrorReadLine();
            for (var i = 0; i < 100 && !server.HasExited; i++) {
                try { using var response = await anonymous.GetAsync("health"); if (response.IsSuccessStatusCode) return; }
                catch (HttpRequestException) { }
                await Task.Delay(200);
            }
            throw new InvalidOperationException("Demo verification API did not restart.");
        }
        async Task Seed(string environment, bool expectedSuccess) {
            using var child = new Process { StartInfo = new ProcessStartInfo("dotnet") {
                WorkingDirectory = server.StartInfo.WorkingDirectory,
                UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true
            } };
            child.StartInfo.ArgumentList.Add(server.StartInfo.ArgumentList[0]);
            child.StartInfo.ArgumentList.Add("--seed-demo-data");
            foreach (var pair in server.StartInfo.Environment) child.StartInfo.Environment[pair.Key] = pair.Value;
            child.StartInfo.Environment["ASPNETCORE_ENVIRONMENT"] = environment;
            child.StartInfo.Environment["Logging__LogLevel__Default"] = "Warning";
            child.Start();
            var output = child.StandardOutput.ReadToEndAsync();
            var error = child.StandardError.ReadToEndAsync();
            await child.WaitForExitAsync();
            var text = await output + await error;
            if ((child.ExitCode == 0) != expectedSuccess) throw new InvalidOperationException("Demo seed CLI: " + text);
            Check(true, expectedSuccess ? "Demo seed CLI completes" : "Production demo seeding is rejected");
        }
        await Restart("Development");
        // Reproduces the original failure: Identity requires a unique email, including demo accounts.
        var firstToken = (await Expect(anonymous, "POST", "dev-session/Admin", new { }, 200))!["accessToken"]!.GetValue<string>();
        Check(await db.Users.Where(u => u.Id.StartsWith(prefix)).AllAsync(u => u.Email != null && u.Email.EndsWith("@demo.example.invalid")),
            "All demo login identities have valid unique emails");
        var originalPatientId = await db.Patients.Where(p => !p.Id.StartsWith(prefix)).Select(p => p.Id).FirstAsync();
        await Seed("Development", true);
        var firstCreatedAt = await db.Activities.Where(a => a.Id == prefix + "activity-0100").Select(a => a.CreatedAt).SingleAsync();
        await Seed("Development", true);
        Check(await db.Patients.CountAsync() == 100 && await db.Practitioners.CountAsync() == 50 && await db.Activities.CountAsync() == 1000,
            "Exact 100/50/1000 totals after repeated seeding");
        Check(await db.Patients.AnyAsync(p => p.Id == originalPatientId), "Existing patient preserved");
        Check(await db.Activities.Where(a => a.Id == prefix + "activity-0100").Select(a => a.CreatedAt).SingleAsync() == firstCreatedAt,
            "Repeat seed preserves existing demo records");
        Check(!await db.Activities.AnyAsync(a => a.PatientActivities.Count != 1 || a.ActivityPractitioners.Count != 1),
            "Every generated activity has patient and practitioner links");
        Check(await db.PractitionerWorkingHours.CountAsync() == 350 && await db.PractitionerBookingSettings.CountAsync() == 50,
            "50 booking settings and 350 working-day rows");
        foreach (var role in new[] { "Admin", "AdmissionsOffice", "Practitioner", "Patient" }) {
            var token = (await Expect(anonymous, "POST", "dev-session/" + role, new { }, 200))!["accessToken"]!.GetValue<string>();
            using var http = new HttpClient { BaseAddress = anonymous.BaseAddress };
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var me = await Expect(http, "GET", "session/me", null, 200);
            Check(me!["roles"]!.AsArray().Any(r => r!.GetValue<string>() == role), "Demo principal role: " + role);
            var visible = (await Expect(http, "GET", "activities", null, 200))!.AsArray();
            Check(role is "Admin" or "AdmissionsOffice" ? visible.Count == 1000 : visible.Count > 0 && visible.Count < 1000,
                "Demo role has scoped events: " + role);
            if (role == "Patient") {
                Check((await Expect(http, "GET", "patients", null, 200))!.AsArray().Count == 1, "Demo patient only sees own profile");
                await Expect(http, "GET", "patients/" + originalPatientId, null, 404);
            }
        }
        await Seed("Production", false);
        await Restart("Production");
        using var oldDemo = new HttpClient { BaseAddress = anonymous.BaseAddress };
        oldDemo.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", firstToken);
        await Expect(oldDemo, "GET", "session/me", null, 401);
        await Expect(anonymous, "POST", "dev-session/Admin", new { }, 404);
    }
    private static async Task<HttpClient> Login(Uri address, string userName, string password)
    {
        var http = new HttpClient { BaseAddress = address };
        var result = await Expect(http, "POST", "session/login", new { userName, password }, 200);
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", result!["accessToken"]!.GetValue<string>());
        return http;
    }
    private static async Task<JsonNode?> Expect(HttpClient http, string method, string path, object? body, int status)
    {
        using var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (body is not null) request.Content = JsonContent.Create(body);
        using var response = await http.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();
        if ((int)response.StatusCode != status) throw new InvalidOperationException($"{method} {path}: expected {status}, got {(int)response.StatusCode}: {text}");
        Check(true, $"{method} {path.Split('/')[0]} -> {status}");
        return string.IsNullOrWhiteSpace(text) ? null : response.Content.Headers.ContentType?.MediaType == "text/plain" ? JsonValue.Create(text) : JsonNode.Parse(text);
    }
    private static void VerifyForms(string root)
    {
        Application.SetHighDpiMode(HighDpiMode.SystemAware); Application.EnableVisualStyles();
        var output = Path.Combine(root, "artifacts", "verification");
        Directory.CreateDirectory(output);
        foreach (var type in new[] { typeof(Form1), typeof(PatientEditForm), typeof(ActivityEditForm), typeof(AppointmentForm), typeof(RecordsForm) })
        {
            using var form = (Form)Activator.CreateInstance(type)!;
            // A top-level WinForms handle megadása kötelező for DrawToBitmap to render child controls.
            _ = form.Handle;
            foreach (Control child in form.Controls) _ = child.Handle;
            form.PerformLayout();
            using var bitmap = new Bitmap(form.Width, form.Height);
            form.DrawToBitmap(bitmap, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
            bitmap.Save(Path.Combine(output, type.Name + ".png"), ImageFormat.Png);
            Check(form.Controls.Count > 0 && form.Width > 300, "Designer form constructs without API: " + type.Name);
        }
    }
}
