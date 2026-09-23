using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Appeals.Queries;

public class AppealDto
{
    public Guid Id { get; set; }
    public Guid? ReportId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public List<string> Evidences { get; set; } = new();
}

public class GetAppealsQuery : IRequest<PagedResult<AppealDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public AppealStatus? Status { get; set; }
}

public class GetAppealsQueryHandler : IRequestHandler<GetAppealsQuery, PagedResult<AppealDto>>
{
    private readonly IAppDbContext _context;

    public GetAppealsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AppealDto>> Handle(GetAppealsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appeals
            .Include(a => a.User)
            .Include(a => a.Evidences)
            .AsQueryable();

        if (request.Status.HasValue)
        {
            query = query.Where(a => a.Status == request.Status.Value);
        }

        var total = await query.CountAsync(cancellationToken);

        var appeals = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = appeals.Select(a => new AppealDto
        {
            Id = a.Id,
            ReportId = a.ReportId,
            UserId = a.UserId,
            UserName = a.User.DisplayName ?? "Unknown",
            Description = a.Description,
            Status = a.Status.ToString(),
            CreatedAt = a.CreatedAt,
            Evidences = a.Evidences.Select(e => e.MediaUrl).ToList()
        }).ToList();

        return new PagedResult<AppealDto>
        {
            Items = dtos,
            TotalCount = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}

