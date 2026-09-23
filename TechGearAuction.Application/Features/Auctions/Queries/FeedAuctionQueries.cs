using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Application.Common.Models;

namespace TechGearAuction.Application.Features.Auctions.Queries;

public class GetFeedAuctionsQuery : IRequest<PagedResult<AuctionDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetFeedAuctionsQueryHandler : IRequestHandler<GetFeedAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetFeedAuctionsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AuctionDto>> Handle(GetFeedAuctionsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty) throw new UnauthorizedAccessException();

        var followedSellerIds = await _context.UserFollows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FolloweeId)
            .ToListAsync(cancellationToken);

        var query = _context.Auctions
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .Include(a => a.Images)
            .Where(a => a.Status == AuctionStatus.Active && followedSellerIds.Contains(a.SellerId))
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var auctions = await query
            .OrderByDescending(a => a.StartTime)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var userWatchlists = await _context.AuctionWatches
            .Where(w => w.UserId == currentUserId)
            .Select(w => w.AuctionId)
            .ToListAsync(cancellationToken);

        var dtos = auctions.Select(a => new AuctionDto
        {
            Id = a.Id,
            CategoryId = a.CategoryId,
            Title = a.Title,
            StartPrice = a.StartPrice,
            CurrentPrice = a.CurrentPrice,
            BuyNowPrice = a.BuyNowPrice,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status.ToString(),
            CategoryName = a.Category.Name,
            SellerName = a.Seller.DisplayName ?? "Anonymous",
            PrimaryImageUrl = a.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl,
            IsWatched = userWatchlists.Contains(a.Id),
            IsFollowedSeller = true
        }).ToList();

        return new PagedResult<AuctionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}

public class GetWatchlistEndingSoonQuery : IRequest<List<AuctionDto>> { }

public class GetWatchlistEndingSoonQueryHandler : IRequestHandler<GetWatchlistEndingSoonQuery, List<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetWatchlistEndingSoonQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<AuctionDto>> Handle(GetWatchlistEndingSoonQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == Guid.Empty) throw new UnauthorizedAccessException();

        var now = DateTime.UtcNow;
        var next24h = now.AddHours(24);

        var watchlists = await _context.AuctionWatches
            .Include(w => w.Auction)
                .ThenInclude(a => a.Category)
            .Include(w => w.Auction)
                .ThenInclude(a => a.Seller)
            .Include(w => w.Auction)
                .ThenInclude(a => a.Images)
            .Where(w => w.UserId == currentUserId && w.Auction.Status == AuctionStatus.Active && w.Auction.EndTime > now && w.Auction.EndTime <= next24h)
            .OrderBy(w => w.Auction.EndTime)
            .ToListAsync(cancellationToken);

        var followedSellerIds = await _context.UserFollows
            .Where(f => f.FollowerId == currentUserId)
            .Select(f => f.FolloweeId)
            .ToListAsync(cancellationToken);

        return watchlists.Select(w => new AuctionDto
        {
            Id = w.Auction.Id,
            CategoryId = w.Auction.CategoryId,
            Title = w.Auction.Title,
            StartPrice = w.Auction.StartPrice,
            CurrentPrice = w.Auction.CurrentPrice,
            BuyNowPrice = w.Auction.BuyNowPrice,
            StartTime = w.Auction.StartTime,
            EndTime = w.Auction.EndTime,
            Status = w.Auction.Status.ToString(),
            CategoryName = w.Auction.Category.Name,
            SellerName = w.Auction.Seller.DisplayName ?? "Anonymous",
            PrimaryImageUrl = w.Auction.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl,
            IsWatched = true,
            IsFollowedSeller = followedSellerIds.Contains(w.Auction.SellerId)
        }).ToList();
    }
}
