using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // EF Core APIs (AnyAsync)
using phonemanagement.Models;

namespace phonemanagement.Data; // namespace gia data layer (seed)

public static class DbSeeder // helper class gia arxiko gemisma tis vasis
{
    public static async Task SeedAsync(AppDbContext db, PasswordHasher<AppUser> hasher) // method pou kanei insert demo data an einai adeia i vasi
    {
        // Ensure admin exists
        const string adminEmail = "admin@test.com";
        var admin = await db.Users.SingleOrDefaultAsync(u => u.Email == adminEmail);
        if (admin is null)
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
            // NOTE: requirement wants admin allowed with 5-char password "admin"
            admin.PasswordHash = hasher.HashPassword(admin, "admin");
            db.Users.Add(admin);
            await db.SaveChangesAsync();
        }
        else if (!string.Equals(admin.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            admin.Role = "Admin";
            await db.SaveChangesAsync();
        }

        // Seed directory users (these become the shared contacts list)
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

