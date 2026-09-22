using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class ToggleUserBlockCommand : IRequest<bool>
{
    public Guid BlockedId { get; set; }
}

public class ToggleUserBlockCommandHandler : IRequestHandler<ToggleUserBlockCommand, bool>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ToggleUserBlockCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<bool> Handle(ToggleUserBlockCommand request, CancellationToken cancellationToken)
    {
        var blockerId = _currentUserService.UserId;

        if (blockerId == request.BlockedId)
        {
            throw new ArgumentException("You cannot block yourself.");
        }

        var blockedUserExists = await _context.Users.AnyAsync(u => u.Id == request.BlockedId, cancellationToken);
        if (!blockedUserExists)
        {
            throw new Exception("User not found.");
        }

        var existingBlock = await _context.UserBlocks
            .FirstOrDefaultAsync(b => b.BlockerId == blockerId && b.BlockedId == request.BlockedId, cancellationToken);

        if (existingBlock != null)
        {
            // Unblock
            _context.UserBlocks.Remove(existingBlock);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }
        else
        {
            // Block
            var block = new UserBlock
            {
                BlockerId = blockerId,
                BlockedId = request.BlockedId
            };
            _context.UserBlocks.Add(block);

            // Break any existing follows in BOTH directions
            var followsToBreak = await _context.UserFollows
                .Where(f => (f.FollowerId == blockerId && f.FolloweeId == request.BlockedId) ||
                            (f.FollowerId == request.BlockedId && f.FolloweeId == blockerId))
                .ToListAsync(cancellationToken);

            if (followsToBreak.Any())
            {
                _context.UserFollows.RemoveRange(followsToBreak);
            }

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
