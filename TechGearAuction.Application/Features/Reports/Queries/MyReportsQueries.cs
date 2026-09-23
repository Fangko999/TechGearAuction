using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Reports.Queries;

public class GetMyReportsQuery : IRequest<PagedResult<ReportDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetMyReportsQueryHandler : IRequestHandler<GetMyReportsQuery, PagedResult<ReportDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyReportsQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<ReportDto>> Handle(GetMyReportsQuery request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        var query = _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Where(r => r.ReporterId == currentUserId)
            .AsQueryable();

        var total = await query.CountAsync(cancellationToken);
        
        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = reports.Select(r => new ReportDto
        {
            Id = r.Id,
            ReporterId = r.ReporterId,
            ReporterName = r.Reporter.DisplayName ?? "Unknown",
            ReportedUserId = r.ReportedUserId,
            ReportedUserName = r.ReportedUser.DisplayName ?? "Unknown",
            AuctionId = r.AuctionId,
            ChatRoomId = r.ChatRoomId,
            Type = r.Type.ToString(),
            Status = r.Status.ToString(),
            Description = r.Description,
            CreatedAt = r.CreatedAt
        }).ToList();

        return new PagedResult<ReportDto>
        {
            Items = items,
            TotalCount = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}

