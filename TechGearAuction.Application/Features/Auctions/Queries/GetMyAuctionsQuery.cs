using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Queries;

public class GetMyAuctionsQuery : IRequest<PagedResult<AuctionDto>>
{
    public string? Status { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetMyAuctionsQueryHandler : IRequestHandler<GetMyAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAuctionsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<AuctionDto>> Handle(GetMyAuctionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Auctions
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .Where(a => a.SellerId == _currentUserService.UserId)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<AuctionStatus>(request.Status, true, out var statusEnum))
        {
            query = query.Where(a => a.Status == statusEnum);
        }

        query = query.OrderByDescending(a => a.CreatedAt);

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

