using MediatR;
using TechGearAuction.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TechGearAuction.Application.Features.Notifications.Commands;

public class MarkNotificationReadCommand : IRequest
{
    public Guid NotificationId { get; set; }
}

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public MarkNotificationReadCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId && n.UserId == userId, cancellationToken);

        if (notification == null)
        {
            throw new KeyNotFoundException("Notification not found or does not belong to the current user.");
        }

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
    }
}

