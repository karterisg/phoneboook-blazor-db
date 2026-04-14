using Microsoft.EntityFrameworkCore;
using PhoneBookApp.Models;

namespace phonemanagement.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Contact> Contacts => Set<Contact>();

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
    }
}

