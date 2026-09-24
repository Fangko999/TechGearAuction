using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetMyBlockedUsersQuery : IRequest<PagedResult<PublicProfileDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetMyBlockedUsersQueryHandler : IRequestHandler<GetMyBlockedUsersQuery, PagedResult<PublicProfileDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyBlockedUsersQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<PublicProfileDto>> Handle(GetMyBlockedUsersQuery request, CancellationToken cancellationToken)
    {
        var blockerId = _currentUserService.UserId;

        var query = _context.UserBlocks.AsNoTracking()
            .Where(b => b.BlockerId == blockerId)
            .Include(b => b.Blocked)
                .ThenInclude(u => u.SocialLinks)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => b.Blocked);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = users.Select(user => new PublicProfileDto
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
        }).ToList();

        return new PagedResult<PublicProfileDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}

