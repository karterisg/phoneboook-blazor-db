namespace phonemanagement.Dtos.Directory;

public sealed record DirectoryContactResponse(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    string Gender);

