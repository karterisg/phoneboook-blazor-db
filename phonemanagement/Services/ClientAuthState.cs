using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace phonemanagement.Services;

public sealed class ClientAuthState
{
    private const string StorageKey = "access_token";

    private string? _accessToken;
    private bool _loaded;
    private readonly ProtectedSessionStorage _storage;

    public event Action? Changed;

    public string? AccessToken => _accessToken;

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_accessToken);

    public ClientAuthState(ProtectedSessionStorage storage)
    {
        _storage = storage;
    }

    public async Task LoadAsync()
    {
        if (_loaded)
            return;

        // If we already have a token in-memory (same circuit), don't risk overwriting it
        // with a storage read that may fail early in the rendering lifecycle.
        if (!string.IsNullOrWhiteSpace(_accessToken))
        {
            _loaded = true;
            Changed?.Invoke();
            return;
        }

        try
        {
            var res = await _storage.GetAsync<string>(StorageKey);
            _accessToken = res.Success ? (string.IsNullOrWhiteSpace(res.Value) ? null : res.Value) : null;
        }
        catch
        {
            // If storage isn't available yet (JS interop timing), DON'T mark as loaded.
            // We'll retry on a later render.
            return;
        }

        _loaded = true;
        Changed?.Invoke();
    }

    public bool IsAdmin
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_accessToken))
                return false;

            try
            {
                var jwt = new JwtSecurityTokenHandler().ReadJwtToken(_accessToken);
                var role = jwt.Claims.FirstOrDefault(c =>
                    string.Equals(c.Type, ClaimTypes.Role, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase))?.Value;

                return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }

    public async Task SetAccessTokenAsync(string? token)
    {
        token = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        _accessToken = token;
        _loaded = true;
        try
        {
            if (token is null)
                await _storage.DeleteAsync(StorageKey);
            else
                await _storage.SetAsync(StorageKey, token);
        }
        catch
        {
            // ignore persistence errors
        }
        Changed?.Invoke();
    }

    public void Apply(HttpRequestMessage request)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }
}

