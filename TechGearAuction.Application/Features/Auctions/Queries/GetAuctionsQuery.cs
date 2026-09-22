using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Auctions.Queries;

public class GetAuctionsQuery : IRequest<PagedResult<AuctionDto>>
{
    public string? Keyword { get; set; }
    public Guid? CategoryId { get; set; }
    public AuctionStatus? Status { get; set; } = AuctionStatus.Active;
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    
    // Sort options: EndingSoon, NewlyListed, PriceAsc, PriceDesc
    public string SortBy { get; set; } = "EndingSoon"; 
    
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetAuctionsQueryHandler : IRequestHandler<GetAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAppDbContext _context;

    public GetAuctionsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuctionDto>> Handle(GetAuctionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Auctions
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .AsQueryable();

        // 1. Filter by Status
        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        // 2. Filter by Category
        if (request.CategoryId.HasValue)
        {
            query = query.Where(a => a.CategoryId == request.CategoryId.Value);
        }

        // 3. Filter by Keyword (Title or Description)
        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            var kw = request.Keyword.ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(kw) || 
                                     (a.Description != null && a.Description.ToLower().Contains(kw)));
        }

        // 4. Filter by Price
        if (request.MinPrice.HasValue)
        {
            query = query.Where(a => a.CurrentPrice >= request.MinPrice.Value);
        }
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(a => a.CurrentPrice <= request.MaxPrice.Value);
        }

        // 5. Sorting
        query = request.SortBy switch
        {
            "EndingSoon" => query.OrderBy(a => a.EndTime),
            "NewlyListed" => query.OrderByDescending(a => a.CreatedAt),
            "PriceAsc" => query.OrderBy(a => a.CurrentPrice),
            "PriceDesc" => query.OrderByDescending(a => a.CurrentPrice),
            _ => query.OrderBy(a => a.EndTime)
        };

        // 6. Pagination
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
