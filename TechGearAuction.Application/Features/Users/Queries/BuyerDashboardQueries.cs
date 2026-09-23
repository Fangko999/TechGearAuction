using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Application.Common.Models;

namespace TechGearAuction.Application.Features.Users.Queries;

public class ActiveBidDto
{
    public Guid AuctionId { get; set; }
    public string Title { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public decimal MyMaxBid { get; set; }
    public decimal CurrentPrice { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsWinning { get; set; }
}

public class GetMyActiveBidsQuery : IRequest<List<ActiveBidDto>> { }

public class GetMyActiveBidsQueryHandler : IRequestHandler<GetMyActiveBidsQuery, List<ActiveBidDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyActiveBidsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ActiveBidDto>> Handle(GetMyActiveBidsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var activeAuctions = await _context.Auctions
            .Include(a => a.Images)
            .Include(a => a.Bids)
            .Where(a => a.Status == AuctionStatus.Active && a.Bids.Any(b => b.BidderId == currentUserId && !b.IsCanceled))
            .ToListAsync(cancellationToken);

        return activeAuctions.Select(a =>
        {
            var myBids = a.Bids.Where(b => b.BidderId == currentUserId && !b.IsCanceled).ToList();
            var myMaxBid = myBids.Any() ? myBids.Max(b => b.BidAmount) : 0;
            
            var highestBid = a.Bids.Where(b => !b.IsCanceled).OrderByDescending(b => b.BidAmount).FirstOrDefault();
            var isWinning = highestBid != null && highestBid.BidderId == currentUserId;

            return new ActiveBidDto
            {
                AuctionId = a.Id,
                Title = a.Title,
                ImageUrl = a.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl,
                MyMaxBid = myMaxBid,
                CurrentPrice = a.CurrentPrice,
                EndTime = a.EndTime,
                IsWinning = isWinning
            };
        }).OrderBy(a => a.EndTime).ToList();
    }
}

public class GetMyWonAuctionsQuery : IRequest<PagedResult<AuctionDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetMyWonAuctionsQueryHandler : IRequestHandler<GetMyWonAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyWonAuctionsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AuctionDto>> Handle(GetMyWonAuctionsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var query = _context.Auctions
            .Include(a => a.Category)
            .Include(a => a.Seller)
            .Include(a => a.Images)
            .Where(a => a.Status == AuctionStatus.Completed && a.WinnerId == currentUserId)
            .AsQueryable();

        var totalCount = await query.CountAsync(cancellationToken);

        var auctions = await query
            .OrderByDescending(a => a.UpdatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
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
            PrimaryImageUrl = a.Images.OrderByDescending(i => i.IsPrimary).FirstOrDefault()?.ImageUrl
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
