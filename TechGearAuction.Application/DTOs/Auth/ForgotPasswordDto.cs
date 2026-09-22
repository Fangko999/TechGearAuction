using System.ComponentModel.DataAnnotations;

namespace TechGearAuction.Application.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}
