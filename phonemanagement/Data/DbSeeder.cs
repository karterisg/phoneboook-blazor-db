using Microsoft.EntityFrameworkCore;
using PhoneBookApp.Models;

namespace phonemanagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Contacts.AnyAsync()) return;

        db.Contacts.AddRange(new[]
        {
            new Contact { Name = "Γιώργος", Phone = "6912345678", Gender = "Male" },
            new Contact { Name = "Κώστας", Phone = "6971234567", Gender = "Male" },
            new Contact { Name = "Λευτέρης", Phone = "6998765432", Gender = "Male" },
            new Contact { Name = "Μαρία", Phone = "6934567890", Gender = "Female" },
            new Contact { Name = "Ελένη", Phone = "6945678901", Gender = "Female" },
            new Contact { Name = "Νίκος", Phone = "6956789012", Gender = "Male" },
            new Contact { Name = "Δημήτρης", Phone = "6967890123", Gender = "Male" },
            new Contact { Name = "Αντώνης", Phone = "6978901234", Gender = "Male" },
            new Contact { Name = "Παναγιώτης", Phone = "6989012345", Gender = "Male" },
            new Contact { Name = "Σοφία", Phone = "6990123456", Gender = "Female" },
            new Contact { Name = "Χρήστος", Phone = "6911122233", Gender = "Male" },
            new Contact { Name = "Βασίλης", Phone = "6922233344", Gender = "Male" },
            new Contact { Name = "Αγγελική", Phone = "6933344455", Gender = "Female" },
            new Contact { Name = "Ιωάννα", Phone = "6944455566", Gender = "Female" },
            new Contact { Name = "Θανάσης", Phone = "6955566677", Gender = "Male" }
        });

        await db.SaveChangesAsync();
    }
}

