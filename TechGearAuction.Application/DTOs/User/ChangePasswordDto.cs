using System.ComponentModel.DataAnnotations;

namespace TechGearAuction.Application.DTOs.User;

public record ChangePasswordDto
{
    [Required]
    public string OldPassword { get; set; } = null!;

    [Required, MinLength(6, ErrorMessage = "New password must be at least 6 characters long.")]
    public string NewPassword { get; set; } = null!;
}


