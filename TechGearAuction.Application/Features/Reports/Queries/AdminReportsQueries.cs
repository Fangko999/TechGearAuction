using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Reports.Queries;

public class ReportDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = null!;
    public Guid ReportedUserId { get; set; }
    public string ReportedUserName { get; set; } = null!;
    public Guid? AuctionId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public string Type { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Evidences { get; set; } = new();
}

public class GetReportsQuery : IRequest<PagedResult<ReportDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public ReportStatus? Status { get; set; }
}

public class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, PagedResult<ReportDto>>
{
    private readonly IAppDbContext _context;

    public GetReportsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<ReportDto>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(r => r.Status == request.Status.Value);
        }

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

public class GetReportByIdQuery : IRequest<ReportDto>
{
    public Guid Id { get; set; }
}

public class GetReportByIdQueryHandler : IRequestHandler<GetReportByIdQuery, ReportDto>
{
    private readonly IAppDbContext _context;

    public GetReportByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ReportDto> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
    {
        var report = await _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.ReportedUser)
            .Include(r => r.Evidences)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (report == null)
            throw new Exception("Report not found.");

        return new ReportDto
        {
            Id = report.Id,
            ReporterId = report.ReporterId,
            ReporterName = report.Reporter.DisplayName ?? "Unknown",
            ReportedUserId = report.ReportedUserId,
            ReportedUserName = report.ReportedUser.DisplayName ?? "Unknown",
            AuctionId = report.AuctionId,
            ChatRoomId = report.ChatRoomId,
            Type = report.Type.ToString(),
            Status = report.Status.ToString(),
            Description = report.Description,
            CreatedAt = report.CreatedAt,
            Evidences = report.Evidences.Select(e => e.MediaUrl).ToList()
        };
    }
}

