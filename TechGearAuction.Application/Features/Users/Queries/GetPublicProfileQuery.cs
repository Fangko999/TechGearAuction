using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetPublicProfileQuery : IRequest<PublicProfileDto>
{
    public Guid UserId { get; set; }
}

public class GetPublicProfileQueryHandler : IRequestHandler<GetPublicProfileQuery, PublicProfileDto>
{
    private readonly IAppDbContext _context;

    public GetPublicProfileQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PublicProfileDto> Handle(GetPublicProfileQuery request, CancellationToken cancellationToken)
    {
        // By default, the global query filter will hide deleted (Closed) users.
        var user = await _context.Users
            .Include(u => u.SocialLinks)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null || user.Status == UserStatus.Closed)
        {
            throw new Exception("User not found or account is closed.");
        }

        return new PublicProfileDto
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            AverageRating = user.AverageRating,
            TotalReviews = user.TotalReviews,
            TotalAuctionsCreated = user.TotalAuctionsCreated,
            TotalAuctionsWon = user.TotalAuctionsWon,
            ReportedAsSellerCount = user.ReportedAsSellerCount,
            ReportedAsBuyerCount = user.ReportedAsBuyerCount,
            SocialLinks = user.SocialLinks.Select(link => new SocialLinkDto
            {
                Platform = link.Platform ?? "Unknown",
                Url = link.Url
            }).ToList()
        };
    }
}

