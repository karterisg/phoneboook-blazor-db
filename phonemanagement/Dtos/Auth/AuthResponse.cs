namespace phonemanagement.Dtos.Auth;

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    string Email);

