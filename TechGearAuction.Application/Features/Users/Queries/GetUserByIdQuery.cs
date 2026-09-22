using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetUserByIdQuery : IRequest<UserProfileDto>
{
    public Guid Id { get; set; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserProfileDto>
{
    private readonly IAppDbContext _context;

    public GetUserByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .IgnoreQueryFilters()
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

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


