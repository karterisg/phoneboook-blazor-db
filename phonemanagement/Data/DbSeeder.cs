using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PhoneBookApp.Models;
using phonemanagement.Models;

namespace phonemanagement.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, PasswordHasher<AppUser> hasher)
    {
        const string adminEmail = "admin@test.com";
        const string legacyAdminEmail = "admin@admin.com";

        var admin = await db.Users.SingleOrDefaultAsync(u => u.Email == adminEmail);

        if (admin is null)
        {
            var legacy = await db.Users.SingleOrDefaultAsync(u => u.Email == legacyAdminEmail);
            if (legacy is not null)
            {
                var contact = await db.Contacts.SingleOrDefaultAsync(c => c.Email == legacyAdminEmail);
                if (contact is not null)
                    contact.Email = adminEmail;

                legacy.Email = adminEmail;
                legacy.Role = "Admin";
                legacy.PasswordHash = hasher.HashPassword(legacy, "admin");
                await db.SaveChangesAsync();
                admin = legacy;
            }
            else
            {
                admin = new AppUser
                {
                    Email = adminEmail,
                    Name = "Admin",
                    Phone = "0000000000",
                    Gender = "Male",
                    Role = "Admin",
                    PasswordHash = "TEMP"
                };
                admin.PasswordHash = hasher.HashPassword(admin, "admin");
                db.Users.Add(admin);
                await db.SaveChangesAsync();
            }
        }
        else if (!string.Equals(admin.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            admin.Role = "Admin";
            await db.SaveChangesAsync();
        }

        var seededUsers = new[]
        {
            new { Name = "Γιώργος", Phone = "6912345678", Email = "giorgos@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Κώστας", Phone = "6971234567", Email = "kostas@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Λευτέρης", Phone = "6998765432", Email = "lefteris@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Μαρία", Phone = "6934567890", Email = "maria@test.com", Gender = "Female", Password = "123456" },
            new { Name = "Ελένη", Phone = "6945678901", Email = "eleni@test.com", Gender = "Female", Password = "123456" },
            new { Name = "Νίκος", Phone = "6956789012", Email = "nikos@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Δημήτρης", Phone = "6967890123", Email = "dimitris@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Αντώνης", Phone = "6978901234", Email = "antonis@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Παναγιώτης", Phone = "6989012345", Email = "panagiotis@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Σοφία", Phone = "6990123456", Email = "sofia@test.com", Gender = "Female", Password = "123456" },
            new { Name = "Χρήστος", Phone = "6911122233", Email = "xristos@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Βασίλης", Phone = "6922233344", Email = "vasilis@test.com", Gender = "Male", Password = "123456" },
            new { Name = "Αγγελική", Phone = "6933344455", Email = "aggeliki@test.com", Gender = "Female", Password = "123456" },
            new { Name = "Ιωάννα", Phone = "6944455566", Email = "ioanna@test.com", Gender = "Female", Password = "123456" },
            new { Name = "Θανάσης", Phone = "6955566677", Email = "thanasis@test.com", Gender = "Male", Password = "123456" }
        };

        foreach (var s in seededUsers)
        {
            var email = s.Email.Trim().ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email == email);
            if (exists) continue;

            var u = new AppUser
            {
                Email = email,
                Name = s.Name,
                Phone = s.Phone,
                Gender = s.Gender,
                Role = "User",
                PasswordHash = "TEMP"
            };
            u.PasswordHash = hasher.HashPassword(u, s.Password);
            db.Users.Add(u);
        }

        await db.SaveChangesAsync();
    }
}
