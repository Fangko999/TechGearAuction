using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.DTOs.Auth;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
            await _authService.RegisterAsync(dto);
            return Ok(new { Message = "Registration successful. Please check your email to verify your account." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "Unknown";
            
            var deviceHash = Request.Headers["X-Device-Hash"].FirstOrDefault() 
                ?? "Unknown";

            var result = await _authService.LoginAsync(dto, ipAddress, deviceHash);
            return Ok(result);
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
    {
        var result = await _authService.VerifyEmailAsync(email, token);
        if (!result) return BadRequest(new { Message = "Email verification failed or token expired." });
        return Ok(new { Message = "Email verified successfully. You can now login." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
    {
            await _authService.ForgotPasswordAsync(dto);
            return Ok(new { Message = "If the email is registered, a password reset link has been sent." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
    {
            await _authService.ResetPasswordAsync(dto);
            return Ok(new { Message = "Password has been reset successfully." });
    }
}
