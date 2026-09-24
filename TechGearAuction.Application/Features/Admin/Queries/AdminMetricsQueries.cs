using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Admin.Queries;

public class MetricsOverviewDto
{
    public int TotalUsers { get; set; }
    public int ActiveAuctions { get; set; }
    public decimal TotalRevenue { get; set; }
    public int PendingReports { get; set; }
}

public class GetMetricsOverviewQuery : IRequest<MetricsOverviewDto> { }

public class GetMetricsOverviewQueryHandler : IRequestHandler<GetMetricsOverviewQuery, MetricsOverviewDto>
{
    private readonly IAppDbContext _context;

    public GetMetricsOverviewQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<MetricsOverviewDto> Handle(GetMetricsOverviewQuery request, CancellationToken cancellationToken)
    {
        return new MetricsOverviewDto
        {
            TotalUsers = await _context.Users.AsNoTracking().CountAsync(cancellationToken),
            ActiveAuctions = await _context.Auctions.AsNoTracking().CountAsync(a => a.Status == AuctionStatus.Active, cancellationToken),
            TotalRevenue = await _context.Auctions.AsNoTracking()
                .Where(a => a.Status == AuctionStatus.Completed)
                .SumAsync(a => a.CurrentPrice, cancellationToken),
            PendingReports = await _context.Reports.AsNoTracking().CountAsync(r => r.Status == ReportStatus.Pending, cancellationToken)
        };
    }
}

public class RevenueDataPoint
{
    public string Date { get; set; } = null!;
    public decimal Revenue { get; set; }
}

public class GetRevenueChartQuery : IRequest<List<RevenueDataPoint>>
{
    public int Days { get; set; } = 30;
}

public class GetRevenueChartQueryHandler : IRequestHandler<GetRevenueChartQuery, List<RevenueDataPoint>>
{
    private readonly IAppDbContext _context;

    public GetRevenueChartQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RevenueDataPoint>> Handle(GetRevenueChartQuery request, CancellationToken cancellationToken)
    {
        var startDate = DateTime.UtcNow.AddDays(-request.Days);
        
        var completedAuctions = await _context.Auctions.AsNoTracking()
            .Where(a => a.Status == AuctionStatus.Completed && a.UpdatedAt >= startDate)
            .ToListAsync(cancellationToken);

        return completedAuctions
            .GroupBy(a => a.UpdatedAt?.Date ?? a.EndTime.Date)
            .Select(g => new RevenueDataPoint
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Revenue = g.Sum(a => a.CurrentPrice)
            })
            .OrderBy(d => d.Date)
            .ToList();
    }
}

