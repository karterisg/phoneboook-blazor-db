using System.Net.Http.Json;
using System.Text.Json;

namespace phonemanagement.Services;

// HTTP kliseis sto idio origin + JWT apo ClientAuthState
public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly ClientAuthState _auth;

    public string? LastError { get; private set; }

    public ApiClient(HttpClient http, ClientAuthState auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        LastError = null;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body)
            };
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                LastError = await TryReadErrorAsync(res, ct);
                return default;
            }

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        catch
        {
            LastError = "Request failed.";
            return default;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        LastError = null;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(body)
            };
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                LastError = await TryReadErrorAsync(res, ct);
                return default;
            }

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        catch
        {
            LastError = "Request failed.";
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string url, CancellationToken ct = default)
    {
        LastError = null;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Delete, url);
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                LastError = await TryReadErrorAsync(res, ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            LastError = "Request failed.";
            return false;
        }
    }

    public async Task<TResponse?> GetAsync<TResponse>(string url, CancellationToken ct = default)
    {
        LastError = null;
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                LastError = await TryReadErrorAsync(res, ct);
                return default;
            }

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        catch
        {
            LastError = "Request failed.";
            return default;
        }
    }

    private static async Task<string> TryReadErrorAsync(HttpResponseMessage res, CancellationToken ct)
    {
        try
        {
            var text = await res.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(text))
                return $"{(int)res.StatusCode} {res.ReasonPhrase}";

            // try common { message: "..." } shape
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.ValueKind == JsonValueKind.Object &&
                doc.RootElement.TryGetProperty("message", out var msg) &&
                msg.ValueKind == JsonValueKind.String &&
                !string.IsNullOrWhiteSpace(msg.GetString()))
            {
                return msg.GetString()!;
            }

            return $"{(int)res.StatusCode} {res.ReasonPhrase}: {text}";
        }
        catch
        {
            return $"{(int)res.StatusCode} {res.ReasonPhrase}";
        }
    }
}

