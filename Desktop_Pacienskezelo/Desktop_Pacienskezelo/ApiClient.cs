using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
namespace Desktop_Pacienskezelo;

public sealed class ApiClient : IDisposable
{
    private readonly HttpClient http = new() { Timeout = TimeSpan.FromSeconds(30) };
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    public Session? Session { get; private set; }
    public bool Staff => Session?.Roles.Any(r => r is "Admin" or "AdmissionsOffice") == true;
    public Uri? BaseAddress => http.BaseAddress;
    public async Task Login(string url, string userName, string password)
    {
        if (!Uri.TryCreate(url.TrimEnd('/') + "/", UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("https" or "http") || !string.IsNullOrEmpty(uri.UserInfo) ||
            (uri.Scheme == "http" && !uri.IsLoopback))
            throw new InvalidOperationException("Érvényes HTTPS API-cím szükséges. HTTP csak localhost esetén használható.");
        http.BaseAddress = uri;
        var token = await Post<Token>("session/login", new { userName, password });
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
        Session = await Get<Session>("session/me");
        if (!Session.Roles.Any(r => r is "Admin" or "AdmissionsOffice" or "Practitioner"))
        {
            Logout();
            throw new InvalidOperationException("Ez a kliens egészségügyi dolgozóknak készült. Páciensfiókkal a webes felület használható.");
        }
    }
    public void Logout() { Session = null; http.DefaultRequestHeaders.Authorization = null; }
    public Task<T> Get<T>(string path) => Send<T>(HttpMethod.Get, path);
    public Task<T> Post<T>(string path, object body) => Send<T>(HttpMethod.Post, path, JsonContent.Create(body));
    public async Task Post(string path, object body) => await Send<object?>(HttpMethod.Post, path, JsonContent.Create(body));
    public async Task Put(string path, object body) => await Send<object?>(HttpMethod.Put, path, JsonContent.Create(body));
    public async Task Delete(string path) => await Send<object?>(HttpMethod.Delete, path);
    public async Task Upload(string path, string title, string filePath)
    {
        using var stream = File.OpenRead(filePath);
        if (stream.Length is < 1 or > 5 * 1024 * 1024) throw new InvalidOperationException("A fájl mérete 1 bájt–5 MB lehet.");
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(title), "title");
        form.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));
        await Send<object?>(HttpMethod.Post, path, form);
    }
    public async Task<byte[]> Download(string path)
    {
        using var response = await http.GetAsync(path);
        await Check(response);
        return await response.Content.ReadAsByteArrayAsync();
    }
    private async Task<T> Send<T>(HttpMethod method, string path, HttpContent? content = null)
    {
        using var request = new HttpRequestMessage(method, path) { Content = content };
        using var response = await http.SendAsync(request);
        await Check(response);
        if (response.StatusCode == HttpStatusCode.NoContent || typeof(T) == typeof(object)) return default!;
        return (await response.Content.ReadFromJsonAsync<T>(Json))!;
    }
    private async Task Check(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode) return;
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            Logout();
            throw new InvalidOperationException("Hibás belépési adatok vagy lejárt munkamenet. Jelentkezz be újra.");
        }
        var body = await response.Content.ReadAsStringAsync();
        var message = response.StatusCode switch {
            HttpStatusCode.Forbidden => "Nincs jogosultságod ehhez a művelethez.",
            HttpStatusCode.NotFound => "A rekord már nem létezik vagy nem érhető el.",
            _ => $"A kérés nem sikerült ({(int)response.StatusCode})."
        };
        try {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.ValueKind == JsonValueKind.String) message = doc.RootElement.GetString()!;
            else if (doc.RootElement.TryGetProperty("message", out var m)) message = m.GetString()!;
            else if (doc.RootElement.TryGetProperty("errors", out var errors))
                message = string.Join(Environment.NewLine, errors.EnumerateObject().SelectMany(p => p.Value.EnumerateArray()).Select(e => e.GetString()));
        } catch (JsonException) { /* Nem jelenítünk meg HTML-t vagy szerveroldali stack trace-t. */ }
        throw new InvalidOperationException(message);
    }
    public void Dispose() => http.Dispose();
    private sealed record Token(string AccessToken);
}
