using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Admin.Commands;

public class RemoveFromBlacklistCommand : IRequest
{
    public string DeviceHash { get; set; } = null!;
}

public class RemoveFromBlacklistCommandHandler : IRequestHandler<RemoveFromBlacklistCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RemoveFromBlacklistCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RemoveFromBlacklistCommand request, CancellationToken cancellationToken)
    {
        var device = await _context.BannedDevices.FirstOrDefaultAsync(b => b.DeviceHash == request.DeviceHash, cancellationToken);
        if (device == null)
            throw new Exception("Device not found in blacklist.");

        _context.BannedDevices.Remove(device);

        _context.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminId = _currentUserService.UserId,
            Action = "RemoveFromBlacklist",
            EntityType = "BannedDevice",
            EntityId = Guid.Empty,
            Details = $"Admin removed device hash {request.DeviceHash} from blacklist."
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
