using System.ComponentModel.DataAnnotations;
using System.Net.Cache;

namespace phonemanagement.Models;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(320)]
    public required string Email { get; set; }

    [MaxLength(500)]
    public required string PasswordHash { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<TaskItem> Tasks { get; set; } = new();

}

