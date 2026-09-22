using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userIdClaim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? user?.FindFirst("sub")?.Value
                              ?? user?.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid id))
            {
                return id;
            }
            throw new UnauthorizedAccessException("Invalid token or user ID not found.");
        }
    }

    public string Role
    {
        get
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        }
    }
}


