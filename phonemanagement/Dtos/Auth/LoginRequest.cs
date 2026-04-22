using System.ComponentModel.DataAnnotations;

namespace phonemanagement.Dtos.Auth;

public sealed class LoginRequest
{
    // Backwards/forwards compatible: some clients send Email, newer code may send Identifier
    public string? Identifier { get; set; }

    [Required, EmailAddress, MaxLength(320)]
    public string Email { get; set; } = "";

    [Required, MinLength(5), MaxLength(200)]
    public string Password { get; set; } = "";
}




