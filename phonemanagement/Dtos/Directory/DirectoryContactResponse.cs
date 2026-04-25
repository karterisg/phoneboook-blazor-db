namespace phonemanagement.Dtos.Directory;

// Apantisi GET /api/directory — SharedContactId otan einai grammi apo Contacts (koini karta)
public sealed record DirectoryContactResponse(
    Guid Id,
    string Name,
    string Phone,
    string Email,
    string Gender,
    int? SharedContactId,
    DateTime CreatedAtUtc);
