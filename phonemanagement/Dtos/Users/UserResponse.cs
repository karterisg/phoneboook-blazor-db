namespace phonemanagement.Dtos.Users;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string Role,
    DateTime CreatedAtUtc,
    int TaskCount);

