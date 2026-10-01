using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Persistence;

internal static class DeclarationTests
{
    public static async Task Run(string root)
    {
        var file = Path.Combine(Path.GetTempPath(), "MedActivities_DeclarationTest_" + Guid.NewGuid().ToString("N") + ".db");
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source=" + file).Options;
        using (var db = new AppDbContext(options)) await db.Database.EnsureCreatedAsync();
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(); var port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop();
        var secret = "Test!A9-" + Guid.NewGuid().ToString("N");
        using var process = new Process { StartInfo = new ProcessStartInfo("dotnet") {
            WorkingDirectory = Path.Combine(root, "API"), UseShellExecute = false, CreateNoWindow = true,
            RedirectStandardOutput = true, RedirectStandardError = true
        }};
        process.StartInfo.ArgumentList.Add(Path.Combine(root, "API/bin/Release/net10.0/API.dll"));
        foreach (var (key, value) in new Dictionary<string, string> {
            ["ASPNETCORE_ENVIRONMENT"] = "Production", ["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}",
            ["Database__Provider"] = "Sqlite", ["Database__ApplyMigrations"] = "false", ["Database__SeedDemoData"] = "false",
            ["ConnectionStrings__DefaultConnection"] = "Data Source=" + file,
            ["BootstrapAdmin__Email"] = "admin@declarations.invalid", ["BootstrapAdmin__Password"] = secret,
            ["Logging__EventLog__LogLevel__Default"] = "None",
            ["Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command"] = "Warning"
        }) process.StartInfo.Environment[key] = value;
        var logs = new System.Text.StringBuilder();
        void Capture(string? line) { lock (logs) { logs.AppendLine(line); if (logs.Length > 16000) logs.Remove(0, logs.Length - 16000); } }
        process.OutputDataReceived += (_, e) => Capture(e.Data);
        process.ErrorDataReceived += (_, e) => Capture(e.Data);
        var passed = 0;
        var started = false;
        void Check(bool condition, string message) {
            if (!condition) throw new InvalidOperationException(message);
            passed++; Console.WriteLine("PASS: " + message);
        }
        try
        {
            started = process.Start(); process.BeginOutputReadLine(); process.BeginErrorReadLine();
            using var http = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}/api/"), Timeout = TimeSpan.FromSeconds(5) };
            var ready = false;
            for (var i = 0; i < 80 && !process.HasExited; i++) {
                try { using var health = await http.GetAsync("health"); ready = health.IsSuccessStatusCode; if (ready) break; }
                catch (HttpRequestException) { }
                await Task.Delay(250);
            }
            if (!ready) throw new InvalidOperationException("Test API failed: " + logs);
            async Task<JsonObject> Post(string path, object body, int status) {
                using var response = await http.PostAsJsonAsync(path, body);
                var text = await response.Content.ReadAsStringAsync();
                Check((int)response.StatusCode == status, $"{path}: expected {status}, actual {(int)response.StatusCode}");
                return JsonNode.Parse(text)!.AsObject();
            }
            var patientPolicy = (await http.GetFromJsonAsync<JsonObject>("session/declaration?role=Patient"))!;
            var staffPolicy = (await http.GetFromJsonAsync<JsonObject>("session/declaration?role=Practitioner"))!;
            var officePolicy = (await http.GetFromJsonAsync<JsonObject>("session/declaration?role=AdmissionsOffice"))!;
            Check(staffPolicy["version"]!.ToString() == officePolicy["version"]!.ToString(), "same staff declaration for doctors and admissions");
            object Acceptance(JsonObject policy, bool accepted = true, string name = "Teszt Nyilatkozó") =>
                new { accepted, version = policy["version"]!.ToString(), signedName = name };
            object Register(string role, object? declaration) => new {
                userName = role, email = role + "@declarations.invalid", password = secret,
                name = "Teszt Nyilatkozó", role, tajNumber = role == "Patient" ? "810000001" : "910000001",
                birthDate = "1990-01-01", declaration
            };
            await Post("session/register", Register("Patient", null), 400);
            await Post("session/register", Register("Patient", Acceptance(patientPolicy, false)), 400);
            await Post("session/register", Register("Patient", new { accepted = true, version = "old" }), 400);
            await Post("session/register", Register("Patient", Acceptance(staffPolicy)), 400);
            await Post("session/register", Register("Practitioner", Acceptance(staffPolicy, name: "   ")), 400);
            await Post("session/register", Register("AdmissionsOffice", Acceptance(staffPolicy)), 400);
            using (var db = new AppDbContext(options)) Check(await db.Users.CountAsync() == 1, "invalid sign-ups leave no users");
            var patient = await Post("session/register", Register("Patient", Acceptance(patientPolicy)), 201);
            var doctor = await Post("session/register", Register("Practitioner", Acceptance(staffPolicy)), 201);
            var patientLogin = await Post("session/login", new { userName = "Patient", password = secret }, 200);
            await Post("session/login", new { userName = "Practitioner", password = secret }, 200);
            var welcomeId = patient["welcomeActivityId"]!.ToString();
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", patientLogin["accessToken"]!.ToString());
            var welcomeDetails = (await http.GetFromJsonAsync<JsonObject>("activities/" + welcomeId))!;
            Check(!welcomeDetails["canEditFields"]!.GetValue<bool>(), "patient welcome does not offer the edit button");
            using (var edit = await http.PutAsJsonAsync("activities", new {
                id = welcomeId, title = "Átírt üdvözlet", description = "Módosítási kísérlet",
                date = "2026-10-01T10:00:00", category = "Egyéb", city = "Budapest", venue = "Deák Ferenc tér",
                latitude = 47.497428, longitude = 19.055135
            })) Check(edit.StatusCode == HttpStatusCode.Forbidden, "patient cannot bypass the welcome restriction with a direct API update");
            Check(welcomeDetails["description"]!.ToString().Contains(patientPolicy["text"]!.ToString()) &&
                welcomeDetails["description"]!.ToString().Contains("EgészségÚt ADMIN"), "welcome contains the accepted declaration and signature");
            Check(welcomeDetails["latitude"]!.GetValue<double>() > 47 && welcomeDetails["longitude"]!.GetValue<double>() > 19,
                "welcome has Budapest map coordinates");
            using (var other = await http.GetAsync("activities/" + doctor["welcomeActivityId"]!.ToString()))
                Check(other.StatusCode == HttpStatusCode.NotFound, "patient cannot read another account's welcome");
            var unread = (await http.GetFromJsonAsync<JsonObject>("notifications/unread-count"))!;
            Check(unread["unreadCount"]!.GetValue<int>() == 1, "welcome badge initially has one unread notification");
            var notices = (await http.GetFromJsonAsync<JsonObject>("notifications"))!;
            var noticeId = notices["items"]![0]!["id"]!.ToString();
            using (var read = await http.PutAsJsonAsync("notifications/" + noticeId + "/read", new { })) {
                Check(read.IsSuccessStatusCode && (await read.Content.ReadFromJsonAsync<JsonObject>())!["url"]!.ToString() == "/activities/" + welcomeId,
                    "notification opens the welcome event");
            }
            Check((await http.GetFromJsonAsync<JsonObject>("notifications/unread-count"))!["unreadCount"]!.GetValue<int>() == 0,
                "opening welcome clears unread count");
            Check(!(await http.GetFromJsonAsync<JsonObject>("activities/" + welcomeId))!["canEditFields"]!.GetValue<bool>(),
                "welcome stays read-only after its notification has been read");
            http.DefaultRequestHeaders.Authorization = null;
            await Post("auth/register", Register("Patient", null), 400);
            await Post("auth/login", new { email = "Patient@declarations.invalid", password = secret }, 400);
            var admin = await Post("session/login", new { userName = "admin@declarations.invalid", password = secret }, 200);
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", admin["accessToken"]!.ToString());
            var office = await Post("user-management", new { userName = "office", email = "office@declarations.invalid", name = "Irodai Tesztelő", password = secret, role = "AdmissionsOffice" }, 201);
            http.DefaultRequestHeaders.Authorization = null;
            using (var db = new AppDbContext(options)) Check(!await db.UserClaims.AnyAsync(c => c.UserId == office["id"]!.ToString()), "admin cannot accept on behalf of employee");
            await Post("session/login", new { userName = "office", password = "wrong" }, 401);
            var challenge = await Post("session/login", new { userName = "office", password = secret }, 409);
            Check(challenge["code"]!.ToString() == "declaration_required" && challenge["accessToken"] is null, "no token before acceptance");
            await Post("session/login", new { userName = "office", password = secret, declaration = Acceptance(staffPolicy, name: " ") }, 409);
            await Post("session/login", new { userName = "office", password = secret, declaration = Acceptance(patientPolicy) }, 409);
            await Post("session/login", new { userName = "office", password = secret, declaration = Acceptance(staffPolicy) }, 200);
            await Post("session/login", new { userName = "office", password = secret }, 200);
            using (var db = new AppDbContext(options)) {
                Check(await db.Activities.CountAsync() == 2 && await db.Appointments.CountAsync() == 0,
                    "one welcome per registration, no duplicates on login and no appointments");
                var doctorId = doctor["id"]!.ToString();
                var doctorWelcomeId = doctor["welcomeActivityId"]!.ToString();
                Check(await db.ActivityPractitioners.AnyAsync(a => a.ActivityId == doctorWelcomeId && a.Practitioner.UserId == doctorId),
                    "doctor welcome is linked to the new doctor profile");
                var activity = await db.Activities.SingleAsync(a => a.Id == welcomeId);
                db.ActivityComments.Add(new Domain.ActivityComment { ActivityId = welcomeId, UserId = patient["id"]!.ToString(),
                    DisplayName = "Teszt Nyilatkozó", Body = "Segítséget kérek a használathoz." });
                await db.SaveChangesAsync();
                Check(await db.UserNotifications.AnyAsync(n => n.UserId == activity.CreatedByUserId && n.Kind == "comment" && n.ActivityId == welcomeId),
                    "support admin receives a notification about a welcome-event message");
                var records = await db.UserClaims.Where(c => c.ClaimType!.StartsWith("medactivities:declaration:")).ToListAsync();
                Check(records.Count == 3, "exactly one record per accepted account, none duplicated at login");
                foreach (var record in records) {
                    var value = JsonNode.Parse(record.ClaimValue!)!;
                    Check(DateTimeOffset.TryParse(value["AcceptedAtUtc"]!.ToString(), out _) &&
                        value["TextSha256"]!.ToString().Length == 64 && value["SignedName"]!.ToString() == "Teszt Nyilatkozó",
                        "persisted server timestamp, full name and policy fingerprint");
                }
            }
            Console.WriteLine($"SUCCESS: {passed} declaration checks passed against an isolated SQLite database.");
        }
        finally {
            if (started && !process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); }
            SqliteConnection.ClearAllPools();
            // Only the exact, randomly named database created by this test is removed.
            foreach (var suffix in new[] { "", "-wal", "-shm" }) if (File.Exists(file + suffix)) File.Delete(file + suffix);
        }
    }
}
