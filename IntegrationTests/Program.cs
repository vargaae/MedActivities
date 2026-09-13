using System.Diagnostics;
using System.Drawing.Imaging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
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
            VerifyApi(root).GetAwaiter().GetResult();
            VerifyForms(root);
            Console.WriteLine($"SUCCESS: {passed} checks passed.");
            return 0;
        } catch (Exception ex) { Console.Error.WriteLine("FAIL: " + ex); return 1; }
    }
    private static void Check(bool condition, string name)
    { if (!condition) throw new InvalidOperationException(name); passed++; Console.WriteLine("PASS: " + name); }
    private static async Task VerifyApi(string root)
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
            var doctors = (await Expect(admin, "GET", "practitioners", null, 200))!.AsArray();
            var doctorId = doctors[0]!["id"]!.GetValue<string>();
            await Expect(doctor, "GET", "patients/" + p1, null, 404);
            await Expect(patient, "GET", "patients/" + p1, null, 404);
            await Expect(patient, "POST", "patients", PatientInput("Tiltott", "123123123"), 403);
            await Expect(admin, "PUT", $"patients/{p1}/access/{doctorId}", new {}, 204);
            await Expect(doctor, "GET", "patients/" + p1, null, 200);
            await Expect(admin, "PUT", $"practitioners/{doctorId}/booking-enabled", new { bookingEnabled = true }, 204);
            var day = DateOnly.FromDateTime(DateTime.Today.AddDays(3));
            await Expect(admin, "PUT", $"practitioners/{doctorId}/working-hours", new { dayOfWeek = (int)day.DayOfWeek, isWorkingDay = true, startTime = "08:00:00", endTime = "20:00:00" }, 204);

            object ActivityInput(string? id, string title) => new { id, title, date = day.ToDateTime(new TimeOnly(10,0)), description = "Leírás", category = "Vizsgálat", city = "Budapest", venue = "Rendelő", patientId = p1, practitionerIds = new[] { doctorId }, status = "Scheduled" };
            var activityId = (await Expect(admin, "POST", "activities", ActivityInput(null, "Esemény"), 201))!.GetValue<string>();
            await Expect(doctor, "GET", "activities/" + activityId, null, 200);
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

            var booking = (await Expect(admin, "POST", "appointments", new { patientId = p1, practitionerId = doctorId, date = day, hour = 10, note = "Megmarad" }, 201))!;
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
            // A top-level WinForms handle is required for DrawToBitmap to render child controls.
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
