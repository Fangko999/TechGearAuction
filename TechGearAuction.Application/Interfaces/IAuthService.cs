using TechGearAuction.Application.DTOs.Auth;

namespace TechGearAuction.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string deviceHash);
    Task<bool> VerifyEmailAsync(string email);
    Task ForgotPasswordAsync(ForgotPasswordDto dto);
    Task ResetPasswordAsync(ResetPasswordDto dto);
}
