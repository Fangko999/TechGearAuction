using System.ComponentModel.DataAnnotations;

namespace TechGearAuction.Application.DTOs.Auth;

public record ResetPasswordDto
{
    [Required]
    public string Token { get; set; } = null!;

    [Required, MinLength(6)]
    public string NewPassword { get; set; } = null!;
}


