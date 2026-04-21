using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Dtos.Users;

public sealed class AdminCreateUserRequest
{
    [Required, MaxLength(200)]
    public string Name { get; set; } = "";

    [Required, MaxLength(50)]
    public string Phone { get; set; } = "";

    [Required, MaxLength(20)]
    public string Gender { get; set; } = "Male";

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = "";

    [Required, MinLength(6), MaxLength(200)]
    public string Password { get; set; } = "";

    [MaxLength(50)]
    public string Role { get; set; } = "User";
}

