using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.SuspiciousActivities.Queries;

public class SuspiciousActivityDto
{
    public Guid Id { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? BidderId { get; set; }
    public string? BidderName { get; set; }
    public int BidderSuspiciousCount { get; set; }
    public Guid? SellerId { get; set; }
    public string? SellerName { get; set; }
    public int SellerSuspiciousCount { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceHash { get; set; }
    public string? Reason { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetSuspiciousActivitiesQuery : IRequest<PagedResult<SuspiciousActivityDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsReviewed { get; set; }
}

public class GetSuspiciousActivitiesQueryHandler : IRequestHandler<GetSuspiciousActivitiesQuery, PagedResult<SuspiciousActivityDto>>
{
    private readonly IAppDbContext _context;

    public GetSuspiciousActivitiesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<SuspiciousActivityDto>> Handle(GetSuspiciousActivitiesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SuspiciousActivities
            .Include(a => a.Bidder)
            .Include(a => a.Seller)
            .AsQueryable();

        if (request.IsReviewed.HasValue)
        {
            query = query.Where(a => a.IsReviewed == request.IsReviewed.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var activities = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = activities.Select(a => new SuspiciousActivityDto
        {
            Id = a.Id,
            AuctionId = a.AuctionId,
            BidderId = a.BidderId,
            BidderName = a.Bidder?.DisplayName,
            BidderSuspiciousCount = a.Bidder?.SuspiciousBidderCount ?? 0,
            SellerId = a.SellerId,
            SellerName = a.Seller?.DisplayName,
            SellerSuspiciousCount = a.Seller?.SuspiciousSellerCount ?? 0,
            IpAddress = a.IpAddress,
            DeviceHash = a.DeviceHash,
            Reason = a.Reason,
            IsReviewed = a.IsReviewed,
            CreatedAt = a.CreatedAt
        }).ToList();

        return new PagedResult<SuspiciousActivityDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
