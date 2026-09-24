using System.ComponentModel.DataAnnotations;

namespace TechGearAuction.Application.DTOs.Auth;

public record ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;
}


