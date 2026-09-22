using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class ToggleUserFollowCommand : IRequest<bool>
{
    public Guid FolloweeId { get; set; }
}

public class ToggleUserFollowCommandHandler : IRequestHandler<ToggleUserFollowCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleUserFollowCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ToggleUserFollowCommand request, CancellationToken cancellationToken)
    {
        var followerId = _currentUserService.UserId;

        if (followerId == request.FolloweeId)
        {
            throw new ArgumentException("You cannot follow yourself.");
        }

        var followeeExists = await _context.Users.AnyAsync(u => u.Id == request.FolloweeId, cancellationToken);
        if (!followeeExists)
        {
            throw new Exception("User not found.");
        }

        var existingFollow = await _context.UserFollows
            .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == request.FolloweeId, cancellationToken);

        if (existingFollow != null)
        {
            // Unfollow
            _context.UserFollows.Remove(existingFollow);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
        else
        {
            // Follow
            var follow = new UserFollow
            {
                FollowerId = followerId,
                FolloweeId = request.FolloweeId
            };
            _context.UserFollows.Add(follow);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

