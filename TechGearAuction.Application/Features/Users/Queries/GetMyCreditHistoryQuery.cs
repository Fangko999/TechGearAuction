using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetMyCreditHistoryQuery : IRequest<List<CreditTransactionDto>>
{
}

public class GetMyCreditHistoryQueryHandler : IRequestHandler<GetMyCreditHistoryQuery, List<CreditTransactionDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetMyCreditHistoryQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<CreditTransactionDto>> Handle(GetMyCreditHistoryQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var transactions = await _context.CreditTransactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new CreditTransactionDto
            {
                Id = t.Id,
                UserId = t.UserId,
                Amount = t.Amount,
                Reason = t.Reason,
                AuctionId = t.AuctionId,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return transactions;
    }
}

