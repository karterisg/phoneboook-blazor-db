using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Dtos.Me;

public sealed class MeUpdateRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(50)]
    public string Phone { get; set; } = "";

    [Required, MaxLength(20)]
    public string Gender { get; set; } = "Male";

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = "";
}

