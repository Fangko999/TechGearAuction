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
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault() 
                ?? HttpContext.Connection.RemoteIpAddress?.ToString() 
                ?? "Unknown";
            
            var deviceHash = Request.Headers["X-Device-Hash"].FirstOrDefault() 
                ?? "Unknown";

            var result = await _authService.LoginAsync(dto, ipAddress, deviceHash);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string email)
    {
        var result = await _authService.VerifyEmailAsync(email);
        if (result)
            return Ok(new { Message = "Email verified successfully." });
        
        return BadRequest(new { Message = "Email verification failed." });
    }
}
