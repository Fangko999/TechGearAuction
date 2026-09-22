using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Auth;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IJwtProvider _jwtProvider;
    private readonly IEmailService _emailService;

    public AuthService(IAppDbContext context, IJwtProvider jwtProvider, IEmailService emailService)
    {
        _context = context;
        _jwtProvider = jwtProvider;
        _emailService = emailService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (existingUser != null)
        {
            throw new Exception("Email already exists");
        }

        var user = new User
        {
            Email = dto.Email,
            DisplayName = dto.DisplayName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = UserRole.User,
            Status = UserStatus.Active,
            AvailableCredits = 3, // Thưởng 3 credit cho user mới
            IsEmailVerified = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Gửi email mock
        await _emailService.SendEmailAsync(
            dto.Email, 
            "Verify your TechGearAuction account", 
            $"Welcome {dto.DisplayName}! Please verify your email."
        );

        var token = _jwtProvider.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName ?? string.Empty,
            Role = user.Role.ToString(),
            Token = token
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress, string deviceHash)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            throw new Exception("Invalid credentials");
        }

        if (!user.IsEmailVerified)
        {
            throw new Exception("Please verify your email before logging in.");
        }

        if (user.Status != UserStatus.Active)
        {
            throw new Exception("User account is not active.");
        }

        // Lưu vết IP & Device
        user.LastLoginIp = ipAddress;
        user.LastLoginDeviceHash = deviceHash;
        await _context.SaveChangesAsync();

        var token = _jwtProvider.GenerateToken(user);

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName ?? string.Empty,
            Role = user.Role.ToString(),
            Token = token
        };
    }

    public async Task<bool> VerifyEmailAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return false;

        user.IsEmailVerified = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
