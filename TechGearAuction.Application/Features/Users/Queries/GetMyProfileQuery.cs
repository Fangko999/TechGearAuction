using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetMyProfileQuery : IRequest<UserProfileDto>
{
}

public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, UserProfileDto>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyProfileQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<UserProfileDto> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _context.Users
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

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
}

