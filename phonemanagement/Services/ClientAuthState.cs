using System.Net.Http.Headers;

namespace phonemanagement.Services;

public sealed class ClientAuthState
{
    private string? _accessToken;

    public event Action? Changed;

    public string? AccessToken => _accessToken;

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(_accessToken);

    public void SetAccessToken(string? token)
    {
        token = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        _accessToken = token;
        Changed?.Invoke();
    }

    public void Apply(HttpRequestMessage request)
    {
        if (string.IsNullOrWhiteSpace(_accessToken))
            return;

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
    }
}

