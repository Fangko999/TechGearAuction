using System.ComponentModel.DataAnnotations;

namespace TechGearAuction.Application.DTOs.Auth;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required, MinLength(6)]
    public string Password { get; set; } = null!;

    [Required, MaxLength(50)]
    public string DisplayName { get; set; } = null!;
}