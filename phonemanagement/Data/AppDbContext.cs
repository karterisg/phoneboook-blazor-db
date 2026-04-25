using Microsoft.EntityFrameworkCore;
using PhoneBookApp.Models;
using phonemanagement.Models;

namespace phonemanagement.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var contact = modelBuilder.Entity<Contact>();
        contact.ToTable("Contacts");
        contact.HasKey(c => c.Id);
        contact.Property(c => c.Id).ValueGeneratedOnAdd();
        contact.Property(c => c.Name).HasMaxLength(200);
        contact.Property(c => c.Phone).HasMaxLength(50);
        contact.Property(c => c.Email).HasMaxLength(200);
        contact.Property(c => c.Gender).HasMaxLength(20);
        contact.Property(c => c.IsUserContribution).IsRequired();
        // Filtrarismeno monadiko index mono otan DirectoryListingId den einai null
        contact.HasIndex(c => c.DirectoryListingId)
            .IsUnique()
            .HasFilter("[DirectoryListingId] IS NOT NULL");
        contact.Property(c => c.CreatedAtUtc).IsRequired();

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
