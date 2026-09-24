using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Users.Queries;

public class SellerMetricsOverviewDto
{
    public decimal MonthlyRevenue { get; set; }
    public int PendingOrders { get; set; }
    public int FollowersCount { get; set; }
}

public class GetSellerMetricsOverviewQuery : IRequest<SellerMetricsOverviewDto> { }

public class GetSellerMetricsOverviewQueryHandler : IRequestHandler<GetSellerMetricsOverviewQuery, SellerMetricsOverviewDto>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetSellerMetricsOverviewQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<SellerMetricsOverviewDto> Handle(GetSellerMetricsOverviewQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyRevenue = await _context.Auctions.AsNoTracking()
            .Where(a => a.SellerId == currentUserId && a.Status == AuctionStatus.Completed && a.UpdatedAt >= startOfMonth)
            .SumAsync(a => a.CurrentPrice, cancellationToken);

        // Pending orders can be defined as completed auctions that haven't been shipped/delivered, but we don't have order tracking yet.
        // We'll count completed auctions won by someone in the last 7 days as an approximation.
        var pendingOrders = await _context.Auctions.AsNoTracking()
            .CountAsync(a => a.SellerId == currentUserId && a.Status == AuctionStatus.Completed && a.WinnerId != null && a.UpdatedAt >= DateTime.UtcNow.AddDays(-7), cancellationToken);

        var followersCount = await _context.UserFollows.AsNoTracking()
            .CountAsync(f => f.FolloweeId == currentUserId, cancellationToken);

        return new SellerMetricsOverviewDto
        {
            MonthlyRevenue = monthlyRevenue,
            PendingOrders = pendingOrders,
            FollowersCount = followersCount
        };
    }
}
