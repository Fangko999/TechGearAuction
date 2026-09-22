using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class UnbanUserCommand : IRequest
{
    public Guid TargetUserId { get; set; }
    public string Reason { get; set; } = null!;
}

public class UnbanUserCommandHandler : IRequestHandler<UnbanUserCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UnbanUserCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UnbanUserCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
        if (targetUser == null)
        {
            throw new Exception("User not found.");
        }

        if (targetUser.Status != UserStatus.Banned)
        {
            throw new ArgumentException("User is not banned.");
        }

        targetUser.Status = UserStatus.Active;

        var auditLog = new AdminAuditLog
        {
            AdminId = adminId,
            Action = "Unban User",
            EntityType = "User",
            EntityId = request.TargetUserId,
            Details = $"Admin unbanned user. Reason: {request.Reason}"
        };
        _context.AdminAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

