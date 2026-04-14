using Microsoft.EntityFrameworkCore; // EF Core base classes (DbContext, ModelBuilder, DbSet)
using PhoneBookApp.Models; // Contact entity

namespace phonemanagement.Data; // namespace gia data layer (DbContext/seed)

public sealed class AppDbContext : DbContext // o EF Core DbContext pou antistoixei sto database
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // DI pernaei options (connection/provider)

    public DbSet<Contact> Contacts => Set<Contact>(); // DbSet = pinaka Contacts (query + CRUD)

    protected override void OnModelCreating(ModelBuilder modelBuilder) // mapping rules apo C# entities -> SQL schema
    {
        var contact = modelBuilder.Entity<Contact>(); // pairnoume builder gia tin ontotita Contact
        contact.ToTable("Contacts"); // onoma pinaka sto SQL
        contact.HasKey(c => c.Id); // primary key
        contact.Property(c => c.Id).ValueGeneratedOnAdd(); // identity/auto-increment
        contact.Property(c => c.Name).HasMaxLength(200); // nvarchar(200)
        contact.Property(c => c.Phone).HasMaxLength(50); // nvarchar(50)
        contact.Property(c => c.Email).HasMaxLength(200); // nvarchar(200)
        contact.Property(c => c.Gender).HasMaxLength(20); // nvarchar(20)
    }
}

