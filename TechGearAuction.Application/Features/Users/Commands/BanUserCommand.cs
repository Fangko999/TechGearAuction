using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class BanUserCommand : IRequest
{
    public Guid TargetUserId { get; set; }
    public string Reason { get; set; } = null!;
}

public class BanUserCommandHandler : IRequestHandler<BanUserCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public BanUserCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;
        if (adminId == request.TargetUserId)
        {
            throw new ArgumentException("Admin cannot ban their own account.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
        if (targetUser == null)
        {
            throw new Exception("User not found.");
        }

        if (targetUser.Status == UserStatus.Banned)
        {
            throw new ArgumentException("User is already banned.");
        }

        targetUser.Status = UserStatus.Banned;

        var auditLog = new AdminAuditLog
        {
            AdminId = adminId,
            Action = "Ban User",
            EntityType = "User",
            EntityId = request.TargetUserId,
            Details = $"Admin banned user. Reason: {request.Reason}"
        };
        _context.AdminAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

