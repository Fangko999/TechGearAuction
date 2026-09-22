using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Auctions.Queries;

public class GetMyWatchlistQuery : IRequest<PagedResult<AuctionDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetMyWatchlistQueryHandler : IRequestHandler<GetMyWatchlistQuery, PagedResult<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyWatchlistQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AuctionDto>> Handle(GetMyWatchlistQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var query = _context.AuctionWatches
            .Where(w => w.UserId == userId)
            .Include(w => w.Auction)
                .ThenInclude(a => a.Category)
            .Include(w => w.Auction)
                .ThenInclude(a => a.Images)
            .Include(w => w.Auction)
                .ThenInclude(a => a.Seller)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.Auction);

        var totalCount = await query.CountAsync(cancellationToken);

        var auctions = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = auctions.Select(a => new AuctionDto
        {
            Id = a.Id,
            CategoryId = a.CategoryId,
            CategoryName = a.Category.Name,
            Title = a.Title,
            StartPrice = a.StartPrice,
            CurrentPrice = a.CurrentPrice,
            BuyNowPrice = a.BuyNowPrice,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            Status = a.Status.ToString(),
            PrimaryImageUrl = a.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl 
                              ?? a.Images.FirstOrDefault()?.ImageUrl,
            SellerName = a.Seller.DisplayName ?? a.Seller.Email
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

