using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Appeals.Commands;

public class ResolveAppealCommand : IRequest
{
    public Guid AppealId { get; set; }
    public bool Approve { get; set; }
    public string AdminNote { get; set; } = null!;
}

public class ResolveAppealCommandHandler : IRequestHandler<ResolveAppealCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ResolveAppealCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ResolveAppealCommand request, CancellationToken cancellationToken)
    {
        var appeal = await _context.Appeals
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == request.AppealId, cancellationToken);

        if (appeal == null)
            throw new NotFoundException("Entity", "Appeal not found.");

        appeal.Status = request.Approve ? AppealStatus.Approved : AppealStatus.Rejected;

        if (request.Approve)
        {
            // Unban user
            appeal.User.Status = UserStatus.Active;
            
            // Remove device from blacklist if exists
            if (!string.IsNullOrEmpty(appeal.User.LastLoginDeviceHash))
            {
                var device = await _context.BannedDevices.FirstOrDefaultAsync(b => b.DeviceHash == appeal.User.LastLoginDeviceHash, cancellationToken);
                if (device != null)
                {
                    _context.BannedDevices.Remove(device);
                }
            }

            // Optionally reduce violation count so they don't get auto-banned again immediately
            if (appeal.User.ViolationCount >= 5)
            {
                appeal.User.ViolationCount = 4;
            }
        }

        _context.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminId = _currentUserService.UserId,
            Action = "ResolveAppeal",
            EntityType = "Appeal",
            EntityId = appeal.Id,
            Details = $"Admin {(request.Approve ? "approved" : "rejected")} appeal. Note: {request.AdminNote}"
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}

