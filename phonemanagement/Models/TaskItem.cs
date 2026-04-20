using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Models;

public sealed class TaskItem
{
    public int Id { get; set; }

    [MaxLength(200)]
    public required string Title { get; set; }

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? DueAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public AppUser? User { get; set; }
}

