using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Queries;

public class GetTrendingAuctionsQuery : IRequest<List<AuctionDto>> { }

public class GetTrendingAuctionsQueryHandler : IRequestHandler<GetTrendingAuctionsQuery, List<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetTrendingAuctionsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<AuctionDto>> Handle(GetTrendingAuctionsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var auctions = await _context.Auctions.AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .Include(a => a.Images)
            .Include(a => a.Bids)
            .Where(a => a.Status == AuctionStatus.Active)
            .OrderByDescending(a => a.Bids.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        // Fetch user context if logged in
        var userWatchlists = currentUserId != Guid.Empty
            ? await _context.AuctionWatches.AsNoTracking().Where(w => w.UserId == currentUserId).Select(w => w.AuctionId).ToListAsync(cancellationToken)
            : new List<Guid>();

        var userFollowings = currentUserId != Guid.Empty
            ? await _context.UserSocialLinks.AsNoTracking().Where(l => l.UserId == currentUserId && l.Platform != null).Select(l => l.Platform!).ToListAsync(cancellationToken) // Note: using Follows, wait
            : new List<string>(); // Need to fix this to use UserFollows if it exists

        var follows = currentUserId != Guid.Empty
            ? await _context.UserFollows.AsNoTracking().Where(f => f.FollowerId == currentUserId).Select(f => f.FolloweeId).ToListAsync(cancellationToken)
            : new List<Guid>();

        return auctions.Select(a => new AuctionDto
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
            IsFollowedSeller = follows.Contains(a.SellerId)
        }).ToList();
    }
}

public class GetEndingSoonAuctionsQuery : IRequest<List<AuctionDto>> { }

public class GetEndingSoonAuctionsQueryHandler : IRequestHandler<GetEndingSoonAuctionsQuery, List<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetEndingSoonAuctionsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<AuctionDto>> Handle(GetEndingSoonAuctionsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var now = DateTime.UtcNow;
        var next24h = now.AddHours(24);

        var auctions = await _context.Auctions.AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .Include(a => a.Images)
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime > now && a.EndTime <= next24h)
            .OrderBy(a => a.EndTime)
            .Take(10)
            .ToListAsync(cancellationToken);

        var userWatchlists = currentUserId != Guid.Empty
            ? await _context.AuctionWatches.AsNoTracking().Where(w => w.UserId == currentUserId).Select(w => w.AuctionId).ToListAsync(cancellationToken)
            : new List<Guid>();

        var follows = currentUserId != Guid.Empty
            ? await _context.UserFollows.AsNoTracking().Where(f => f.FollowerId == currentUserId).Select(f => f.FolloweeId).ToListAsync(cancellationToken)
            : new List<Guid>();

        return auctions.Select(a => new AuctionDto
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
            IsFollowedSeller = follows.Contains(a.SellerId)
        }).ToList();
    }
}

public class RecentWinnerDto
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; } = null!;
    public string WinnerName { get; set; } = null!;
    public decimal FinalPrice { get; set; }
    public string? ImageUrl { get; set; }
}

public class GetRecentWinnersQuery : IRequest<List<RecentWinnerDto>> { }

public class GetRecentWinnersQueryHandler : IRequestHandler<GetRecentWinnersQuery, List<RecentWinnerDto>>
{
    private readonly IAppDbContext _context;

    public GetRecentWinnersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentWinnerDto>> Handle(GetRecentWinnersQuery request, CancellationToken cancellationToken)
    {
        var auctions = await _context.Auctions.AsNoTracking()
            .Include(a => a.Winner)
            .Include(a => a.Images)
            .Where(a => a.Status == AuctionStatus.Completed && a.WinnerId != null)
            .OrderByDescending(a => a.UpdatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        return auctions.Select(a => new RecentWinnerDto
        {
            AuctionId = a.Id,
            Title = a.Title,
            WinnerName = a.Winner!.DisplayName ?? "Anonymous",
            FinalPrice = a.CurrentPrice,
            ImageUrl = a.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl
        }).ToList();
    }
}
