using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Admin.Queries;

public class AdminAuditLogDto
{
    public Guid Id { get; set; }
    public Guid AdminId { get; set; }
    public string AdminName { get; set; } = null!;
    public string Action { get; set; } = null!;
    public string EntityType { get; set; } = null!;
    public Guid EntityId { get; set; }
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetAuditLogsQuery : IRequest<PagedResult<AdminAuditLogDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public Guid? AdminId { get; set; }
}

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AdminAuditLogDto>>
{
    private readonly IAppDbContext _context;

    public GetAuditLogsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AdminAuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AdminAuditLogs
            .Include(l => l.Admin)
            .AsQueryable();

        if (request.AdminId.HasValue)
        {
            query = query.Where(l => l.AdminId == request.AdminId.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var logs = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = logs.Select(l => new AdminAuditLogDto
        {
            Id = l.Id,
            AdminId = l.AdminId,
            AdminName = l.Admin.DisplayName ?? "Unknown",
            Action = l.Action,
            EntityType = l.EntityType,
            EntityId = l.EntityId,
            Details = l.Details,
            CreatedAt = l.CreatedAt
        }).ToList();

        return new PagedResult<AdminAuditLogDto>
        {
            Items = dtos,
            TotalCount = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
