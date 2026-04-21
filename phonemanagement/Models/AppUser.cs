using System.ComponentModel.DataAnnotations;
using System.Net.Cache;

namespace phonemanagement.Models;

public sealed class AppUser
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(320)]
    public required string Email { get; set; }

    [MaxLength(200)]
    public string Name { get; set; } = "";

    [MaxLength(50)]
    public string Phone { get; set; } = "";

    [MaxLength(20)]
    public string Gender { get; set; } = "Male";

    [MaxLength(50)]
    public string Role { get; set; } = "User";

    [MaxLength(500)]
    public required string PasswordHash { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<TaskItem> Tasks { get; set; } = new();

}

