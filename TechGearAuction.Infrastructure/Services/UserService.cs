using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;
    private readonly IStorageService _storageService;
    private readonly MinioSettings _minioSettings;

    public UserService(IAppDbContext context, IStorageService storageService, Microsoft.Extensions.Options.IOptions<MinioSettings> minioSettings)
    {
        _context = context;
        _storageService = storageService;
        _minioSettings = minioSettings.Value;
    }

    public async Task UpdateProfileAsync(int userId, UpdateProfileRequestDto dto)
    {
        var user = await _context.Users
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.DisplayName = dto.DisplayName;
        user.PhoneNumber = dto.PhoneNumber;

        // Xóa liên kết cũ
        if (user.SocialLinks.Any())
        {
            _context.UserSocialLinks.RemoveRange(user.SocialLinks);
        }

        // Thêm liên kết mới
        if (dto.SocialLinks != null && dto.SocialLinks.Any())
        {
            var newLinks = dto.SocialLinks.Select(link => new UserSocialLink
            {
                UserId = userId,
                Platform = link.Platform,
                Url = link.Url
            }).ToList();

            _context.UserSocialLinks.AddRange(newLinks);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            AvailableCredits = user.AvailableCredits,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            SocialLinks = user.SocialLinks.Select(link => new SocialLinkDto
            {
                Platform = link.Platform ?? "Unknown",
                Url = link.Url
            }).ToList()
        };
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.PasswordHash))
        {
            throw new ArgumentException("Incorrect old password.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        // UpdatedAt is handled automatically by EF Core via SaveChanges interceptor
        
        await _context.SaveChangesAsync();
    }

    public async Task DeleteUserAsync(int adminId, int targetUserId)
    {
        if (adminId == targetUserId)
        {
            throw new ArgumentException("Admin cannot delete their own account.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (targetUser == null)
        {
            throw new Exception("User not found.");
        }

        targetUser.Status = UserStatus.Banned;
        targetUser.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task UpdateUserProfileByAdminAsync(int adminId, int targetUserId, UpdateProfileRequestDto dto)
    {
        var targetUser = await _context.Users.Include(u => u.SocialLinks).FirstOrDefaultAsync(u => u.Id == targetUserId);
        if (targetUser == null)
        {
            throw new Exception("User not found.");
        }

        targetUser.DisplayName = dto.DisplayName;
        targetUser.PhoneNumber = dto.PhoneNumber;

        if (targetUser.SocialLinks.Any())
        {
            _context.UserSocialLinks.RemoveRange(targetUser.SocialLinks);
        }

        if (dto.SocialLinks != null && dto.SocialLinks.Any())
        {
            var newLinks = dto.SocialLinks.Select(link => new UserSocialLink
            {
                UserId = targetUserId,
                Platform = link.Platform,
                Url = link.Url
            }).ToList();
            _context.UserSocialLinks.AddRange(newLinks);
        }

        // Add audit log
        var auditLog = new AdminAuditLog
        {
            AdminId = adminId,
            Action = "Update Profile",
            EntityType = "User",
            EntityId = targetUserId,
            Details = $"Admin updated profile of user {targetUserId}"
        };
        _context.AdminAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync();
    }

    public async Task<string> UpdateAvatarAsync(int userId, Stream fileStream, string fileName, string contentType)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var extension = Path.GetExtension(fileName);
        var newFileName = $"{userId}_{Guid.NewGuid()}{extension}";
        
        var avatarUrl = await _storageService.UploadFileAsync(fileStream, newFileName, contentType, _minioSettings.Buckets.Avatars);
        
        user.AvatarUrl = avatarUrl;
        await _context.SaveChangesAsync();
        
        return avatarUrl;
    }
}

