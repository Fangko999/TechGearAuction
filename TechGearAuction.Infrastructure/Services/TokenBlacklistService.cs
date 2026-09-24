using Microsoft.Extensions.Caching.Memory;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Infrastructure.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _cache;

    public TokenBlacklistService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void BlacklistToken(string token)
    {
        // Cache the token for 24 hours (maximum token lifetime in this system)
        _cache.Set(token, true, TimeSpan.FromHours(24));
    }

    public bool IsTokenBlacklisted(string token)
    {
        return _cache.TryGetValue(token, out _);
    }
}
