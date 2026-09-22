using TechGearAuction.Application.DTOs.User;

namespace TechGearAuction.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetProfileAsync(int userId);
    Task UpdateProfileAsync(int userId, UpdateProfileRequestDto dto);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto);
    Task DeleteUserAsync(int adminId, int targetUserId);
}

