namespace PhoneBookApp.Models;

// Grammi ston pinaka Contacts (legacy sync me Users + koines kartes directory)
public class Contact
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string Gender { get; set; } = "Male";

    // Karta pou prosthete xristis: emfanizetai se olous sto /api/directory
    public bool IsUserContribution { get; set; }

    // Guid gia GET /api/directory/{id} otan einai koini epafi (null gia palies grammes)
    public Guid? DirectoryListingId { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
