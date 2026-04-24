using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Dtos.Me;

public sealed class ChangePasswordRequest
{
    [Required, MaxLength(200)]
    public string CurrentPassword { get; set; } = "";

    [Required, MinLength(6), MaxLength(200)]
    public string NewPassword { get; set; } = "";
}

