using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IAppDbContext _context;

    public UserService(IAppDbContext context)
    {
        _context = context;
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
        user.AvatarUrl = dto.AvatarUrl;

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
}

