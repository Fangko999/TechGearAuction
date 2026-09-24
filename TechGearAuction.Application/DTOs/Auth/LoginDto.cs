using System.ComponentModel.DataAnnotations;

namespace TechGearAuction.Application.DTOs.Auth;

public record LoginDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}
