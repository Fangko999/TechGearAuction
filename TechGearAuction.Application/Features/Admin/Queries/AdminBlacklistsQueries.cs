using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Admin.Queries;

public class BannedDeviceDto
{
    public string DeviceHash { get; set; } = null!;
    public string? Reason { get; set; }
    public DateTime BannedAt { get; set; }
}

public class GetBlacklistsQuery : IRequest<PagedResult<BannedDeviceDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class GetBlacklistsQueryHandler : IRequestHandler<GetBlacklistsQuery, PagedResult<BannedDeviceDto>>
{
    private readonly IAppDbContext _context;

    public GetBlacklistsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<BannedDeviceDto>> Handle(GetBlacklistsQuery request, CancellationToken cancellationToken)
    {
        var total = await _context.BannedDevices.CountAsync(cancellationToken);

        var devices = await _context.BannedDevices
            .OrderByDescending(d => d.BannedAt)
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = devices.Select(d => new BannedDeviceDto
        {
            DeviceHash = d.DeviceHash,
            Reason = d.Reason,
            BannedAt = d.BannedAt
        }).ToList();

        return new PagedResult<BannedDeviceDto>
        {
            Items = items,
            TotalCount = total,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
