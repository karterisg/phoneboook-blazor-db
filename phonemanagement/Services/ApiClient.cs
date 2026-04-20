using System.Net.Http.Json;

namespace phonemanagement.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;
    private readonly ClientAuthState _auth;

    public ApiClient(HttpClient http, ClientAuthState auth)
    {
        _http = http;
        _auth = auth;
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body)
            };
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                return default;

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest body, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = JsonContent.Create(body)
            };
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                return default;

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        catch
        {
            return default;
        }
    }

    public async Task<bool> DeleteAsync(string url, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Delete, url);
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            return res.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<TResponse?> GetAsync<TResponse>(string url, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            _auth.Apply(req);

            using var res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
                return default;

            return await res.Content.ReadFromJsonAsync<TResponse>(cancellationToken: ct);
        }
        catch
        {
            return default;
        }
    }
}

