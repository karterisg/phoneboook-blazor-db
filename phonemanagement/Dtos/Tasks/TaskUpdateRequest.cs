using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Dtos.Tasks;

public sealed class TaskUpdateRequest
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? DueAtUtc { get; set; }
}

