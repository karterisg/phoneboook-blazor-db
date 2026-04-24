namespace phonemanagement.Dtos.Me;

public sealed record MeResponse(
    Guid Id,
    string Email,
    string Name,
    string Phone,
    string Gender,
    string Role,
    DateTime CreatedAtUtc);

