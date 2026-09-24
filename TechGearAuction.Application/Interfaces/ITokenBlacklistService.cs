namespace TechGearAuction.Application.Interfaces;

public interface ITokenBlacklistService
{
    void BlacklistToken(string token);
    bool IsTokenBlacklisted(string token);
}
