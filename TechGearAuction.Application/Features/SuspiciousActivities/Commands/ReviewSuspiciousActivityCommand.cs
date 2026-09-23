using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.SuspiciousActivities.Commands;

public class ReviewSuspiciousActivityCommand : IRequest
{
    public Guid Id { get; set; }
}

public class ReviewSuspiciousActivityCommandHandler : IRequestHandler<ReviewSuspiciousActivityCommand>
{
    private readonly IAppDbContext _context;

    public ReviewSuspiciousActivityCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReviewSuspiciousActivityCommand request, CancellationToken cancellationToken)
    {
        var activity = await _context.SuspiciousActivities.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);
        
        if (activity == null)
            throw new Exception("Suspicious activity not found.");

        activity.IsReviewed = true;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
