using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.Auction;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Admin.Queries;

public class GetAdminAuctionsQuery : IRequest<PagedResult<AuctionDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public AuctionStatus? Status { get; set; }
}

public class GetAdminAuctionsQueryHandler : IRequestHandler<GetAdminAuctionsQuery, PagedResult<AuctionDto>>
{
    private readonly IAppDbContext _context;

    public GetAdminAuctionsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuctionDto>> Handle(GetAdminAuctionsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Auctions
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Seller)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var auctions = await query
            .OrderByDescending(a => a.CreatedAt)
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
            PrimaryImageUrl = a.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? a.Images.FirstOrDefault()?.ImageUrl
        }).ToList();

        return new PagedResult<AuctionDto>
        {
            Items = dtos,
            TotalCount = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
