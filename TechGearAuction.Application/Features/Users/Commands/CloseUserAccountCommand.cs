using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Users.Commands;

public class CloseUserAccountCommand : IRequest
{
    public Guid TargetUserId { get; set; }
}

public class CloseUserAccountCommandHandler : IRequestHandler<CloseUserAccountCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CloseUserAccountCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CloseUserAccountCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;
        if (adminId == request.TargetUserId)
        {
            throw new ArgumentException("Admin cannot close their own account.");
        }

        var targetUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
        if (targetUser == null)
        {
            throw new NotFoundException("Entity", "User not found.");
        }

        targetUser.Status = UserStatus.Closed;
        targetUser.DeletedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}


