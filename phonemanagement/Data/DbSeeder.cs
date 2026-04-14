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
            new Contact { Name = "Γιώργος", Phone = "6912345678", Email = "giorgos@test.com", Gender = "Male" },
            new Contact { Name = "Κώστας", Phone = "6971234567", Email = "kostas@test.com", Gender = "Male" },
            new Contact { Name = "Λευτέρης", Phone = "6998765432", Email = "lefteris@test.com", Gender = "Male" },
            new Contact { Name = "Μαρία", Phone = "6934567890", Email = "maria@test.com", Gender = "Female" },
            new Contact { Name = "Ελένη", Phone = "6945678901", Email = "eleni@test.com", Gender = "Female" },
            new Contact { Name = "Νίκος", Phone = "6956789012", Email = "nikos@test.com", Gender = "Male" },
            new Contact { Name = "Δημήτρης", Phone = "6967890123", Email = "dimitris@test.com", Gender = "Male" },
            new Contact { Name = "Αντώνης", Phone = "6978901234", Email = "antonis@test.com", Gender = "Male" },
            new Contact { Name = "Παναγιώτης", Phone = "6989012345", Email = "panagiotis@test.com", Gender = "Male" },
            new Contact { Name = "Σοφία", Phone = "6990123456", Email = "sofia@test.com", Gender = "Female" },
            new Contact { Name = "Χρήστος", Phone = "6911122233", Email = "xristos@test.com", Gender = "Male" },
            new Contact { Name = "Βασίλης", Phone = "6922233344", Email = "vasilis@test.com", Gender = "Male" },
            new Contact { Name = "Αγγελική", Phone = "6933344455", Email = "aggeliki@test.com", Gender = "Female" },
            new Contact { Name = "Ιωάννα", Phone = "6944455566", Email = "ioanna@test.com", Gender = "Female" },
            new Contact { Name = "Θανάσης", Phone = "6955566677", Email = "thanasis@test.com", Gender = "Male" }
        });

        db.SaveChanges();

        await db.SaveChangesAsync();
    }
}

