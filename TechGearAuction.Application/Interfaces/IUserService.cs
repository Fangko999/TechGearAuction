using TechGearAuction.Application.DTOs.User;

namespace TechGearAuction.Application.Interfaces;

public interface IUserService
{
    Task UpdateProfileAsync(int userId, UpdateProfileRequestDto dto);
}

