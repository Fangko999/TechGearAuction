using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Interfaces;

public interface IJwtProvider
{
    string GenerateToken(User user);
}

