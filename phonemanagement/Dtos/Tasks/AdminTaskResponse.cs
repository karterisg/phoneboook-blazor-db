namespace phonemanagement.Dtos.Tasks;

public sealed record AdminTaskResponse(
    int Id,
    Guid UserId,
    string UserEmail,
    string UserName,
    string Title,
    string? Notes,
    bool IsCompleted,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc);

