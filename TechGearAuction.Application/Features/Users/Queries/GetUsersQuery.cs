using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetUsersQuery : IRequest<List<UserProfileDto>>
{
    // For simplicity, returning a list without pagination for now, but in reality should use pagination.
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserProfileDto>>
{
    private readonly IAppDbContext _context;

    public GetUsersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserProfileDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Include(u => u.SocialLinks)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        return users.Select(user => new UserProfileDto
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
        }).ToList();
    }
}
