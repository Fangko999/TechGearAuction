using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Users.Commands;

public class RestoreUserAccountCommand : IRequest
{
    public Guid TargetUserId { get; set; }
}

public class RestoreUserAccountCommandHandler : IRequestHandler<RestoreUserAccountCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public RestoreUserAccountCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(RestoreUserAccountCommand request, CancellationToken cancellationToken)
    {
        var targetUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
            
        if (targetUser == null)
        {
            throw new Exception("User not found.");
        }

        if (targetUser.DeletedAt == null)
        {
            throw new ArgumentException("User is already active.");
        }

        targetUser.Status = UserStatus.Active;
        targetUser.DeletedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

