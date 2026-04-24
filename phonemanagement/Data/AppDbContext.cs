using Microsoft.EntityFrameworkCore; // EF Core base classes (DbContext, ModelBuilder, DbSet)
using PhoneBookApp.Models; // Contact entity
using phonemanagement.Models;

namespace phonemanagement.Data; // namespace gia data layer (DbContext/seed)

public sealed class AppDbContext : DbContext // o EF Core DbContext pou antistoixei sto database
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } // DI pernaei options (connection/provider)

    public DbSet<Contact> Contacts => Set<Contact>(); // DbSet = pinaka Contacts (query + CRUD)
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>(); 

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


        var user = modelBuilder.Entity<AppUser>();
        user.ToTable("Users");
        user.HasKey(u => u.Id);
        user.Property(u => u.Email).HasMaxLength(320).IsRequired();
        user.HasIndex(u => u.Email).IsUnique();
        user.Property(u => u.Name).HasMaxLength(200).IsRequired();
        user.Property(u => u.Phone).HasMaxLength(50).IsRequired();
        user.Property(u => u.Gender).HasMaxLength(20).IsRequired();
        user.Property(u => u.Role).HasMaxLength(50).IsRequired();
        user.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        user.Property(u => u.CreatedAtUtc).IsRequired();





        var task = modelBuilder.Entity<TaskItem>();
        task.ToTable("Tasks");
        task.HasKey(t => t.Id);
        task.Property(t => t.Id).ValueGeneratedOnAdd();
        task.Property(t => t.Title).HasMaxLength(200).IsRequired();
        task.Property(t => t.Notes).HasMaxLength(2000);
        task.Property(t => t.CreatedAtUtc).IsRequired();
        task.HasIndex(t => new { t.UserId, t.CreatedAtUtc });
        task.HasOne(t => t.User)
            .WithMany(u => u.Tasks)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

