using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Dtos.Auth;

public sealed class LoginRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = "";

    [Required, MinLength(5), MaxLength(200)]
    public string Password { get; set; } = "";
}

