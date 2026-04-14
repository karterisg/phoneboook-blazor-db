using Microsoft.EntityFrameworkCore; // EF Core APIs (AnyAsync)
using PhoneBookApp.Models; // Contact entity

namespace phonemanagement.Data; // namespace gia data layer (seed)

public static class DbSeeder // helper class gia arxiko gemisma tis vasis
{
    public static async Task SeedAsync(AppDbContext db) // method pou kanei insert demo data an einai adeia i vasi
    {
        if (await db.Contacts.AnyAsync()) return; // an exei idi eggrafes, min ksanavaleis seed

        db.Contacts.AddRange(new[] // vazoume polla contacts me mia kinisi (bulk add)
        {
            new Contact { Name = "Γιώργος", Phone = "6912345678", Email = "giorgos@test.com", Gender = "Male" }, // demo contact 1
            new Contact { Name = "Κώστας", Phone = "6971234567", Email = "kostas@test.com", Gender = "Male" }, // demo contact 2
            new Contact { Name = "Λευτέρης", Phone = "6998765432", Email = "lefteris@test.com", Gender = "Male" }, // demo contact 3
            new Contact { Name = "Μαρία", Phone = "6934567890", Email = "maria@test.com", Gender = "Female" }, // demo contact 4
            new Contact { Name = "Ελένη", Phone = "6945678901", Email = "eleni@test.com", Gender = "Female" }, // demo contact 5
            new Contact { Name = "Νίκος", Phone = "6956789012", Email = "nikos@test.com", Gender = "Male" }, // demo contact 6
            new Contact { Name = "Δημήτρης", Phone = "6967890123", Email = "dimitris@test.com", Gender = "Male" }, // demo contact 7
            new Contact { Name = "Αντώνης", Phone = "6978901234", Email = "antonis@test.com", Gender = "Male" }, // demo contact 8
            new Contact { Name = "Παναγιώτης", Phone = "6989012345", Email = "panagiotis@test.com", Gender = "Male" }, // demo contact 9
            new Contact { Name = "Σοφία", Phone = "6990123456", Email = "sofia@test.com", Gender = "Female" }, // demo contact 10
            new Contact { Name = "Χρήστος", Phone = "6911122233", Email = "xristos@test.com", Gender = "Male" }, // demo contact 11
            new Contact { Name = "Βασίλης", Phone = "6922233344", Email = "vasilis@test.com", Gender = "Male" }, // demo contact 12
            new Contact { Name = "Αγγελική", Phone = "6933344455", Email = "aggeliki@test.com", Gender = "Female" }, // demo contact 13
            new Contact { Name = "Ιωάννα", Phone = "6944455566", Email = "ioanna@test.com", Gender = "Female" }, // demo contact 14
            new Contact { Name = "Θανάσης", Phone = "6955566677", Email = "thanasis@test.com", Gender = "Male" } // demo contact 15
        });

        db.SaveChanges(); // kanei save sync (peritto afou akolouthei SaveChangesAsync)

        await db.SaveChangesAsync(); // kanei save async (to kanoniko pou xreiazetai)
    }
}

