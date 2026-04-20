namespace phonemanagement.Dtos.Tasks;

public sealed record TaskResponse(
    int Id,
    string Title,
    string? Notes,
    bool IsCompleted,
    DateTime? DueAtUtc,
    DateTime CreatedAtUtc);

