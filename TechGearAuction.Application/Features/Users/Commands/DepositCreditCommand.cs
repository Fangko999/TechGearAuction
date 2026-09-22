using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class DepositCreditCommand : IRequest
{
    public int Amount { get; set; }
}

public class DepositCreditCommandHandler : IRequestHandler<DepositCreditCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DepositCreditCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DepositCreditCommand request, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be greater than 0.");
        }

        var userId = _currentUserService.UserId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Add credits
        user.AvailableCredits += request.Amount;

        // Log transaction
        var transaction = new CreditTransaction
        {
            UserId = userId,
            Amount = request.Amount,
            Reason = "Nạp Credit hệ thống",
            AuctionId = null
        };
        
        _context.CreditTransactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);
    }
}

