using MediatR;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace TechGearAuction.Application.Features.Reports.Commands;

public class CreateReportCommand : IRequest<Guid>
{
    public Guid ReportedUserId { get; set; }
    public Guid? AuctionId { get; set; }
    public Guid? ChatRoomId { get; set; }
    public ReportType Type { get; set; }
    public string? Description { get; set; }
}

public class CreateReportCommandHandler : IRequestHandler<CreateReportCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateReportCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var reporterId = _currentUserService.UserId;

        if (reporterId == request.ReportedUserId)
        {
            throw new ArgumentException("You cannot report yourself.");
        }

        var reportedUserExists = await _context.Users.AnyAsync(u => u.Id == request.ReportedUserId, cancellationToken);
        if (!reportedUserExists)
        {
            throw new KeyNotFoundException("Reported user not found.");
        }

        if (request.AuctionId.HasValue)
        {
            var auctionExists = await _context.Auctions.AnyAsync(a => a.Id == request.AuctionId.Value, cancellationToken);
            if (!auctionExists) throw new KeyNotFoundException("Auction not found.");
        }

        if (request.ChatRoomId.HasValue)
        {
            var chatRoomExists = await _context.ChatRooms.AnyAsync(c => c.Id == request.ChatRoomId.Value, cancellationToken);
            if (!chatRoomExists) throw new KeyNotFoundException("Chat room not found.");
        }

        var report = new Report
        {
            ReporterId = reporterId,
            ReportedUserId = request.ReportedUserId,
            AuctionId = request.AuctionId,
            ChatRoomId = request.ChatRoomId,
            Type = request.Type,
            Description = request.Description,
            Status = ReportStatus.Pending
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync(cancellationToken);

        return report.Id;
    }
}

