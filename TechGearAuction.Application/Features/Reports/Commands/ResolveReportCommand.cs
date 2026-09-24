using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Reports.Commands;

public enum ResolutionAction
{
    Reject,
    IssueStrike,
    DirectBan
}

public class ResolveReportCommand : IRequest
{
    public Guid ReportId { get; set; }
    public ResolutionAction Action { get; set; }
    public string? AdminNote { get; set; }
}

public class ResolveReportCommandHandler : IRequestHandler<ResolveReportCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ResolveReportCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ResolveReportCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var report = await _context.Reports
            .Include(r => r.ReportedUser)
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken);

        if (report == null)
            throw new NotFoundException("Entity", "Report not found.");

        var reportedUser = report.ReportedUser;

        if (request.Action == ResolutionAction.Reject)
        {
            report.Status = ReportStatus.Rejected;
        }
        else if (request.Action == ResolutionAction.IssueStrike)
        {
            report.Status = ReportStatus.Resolved;
            reportedUser.ViolationCount++;

            if (reportedUser.ViolationCount >= 5)
            {
                await BanUserLogic(reportedUser, "Auto-ban: Accumulated 5 strikes.");
            }
        }
        else if (request.Action == ResolutionAction.DirectBan)
        {
            report.Status = ReportStatus.Resolved;
            await BanUserLogic(reportedUser, $"Direct ban by admin. Note: {request.AdminNote}");
        }

        _context.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminId = adminId,
            Action = "ResolveReport",
            EntityType = "Report",
            EntityId = request.ReportId,
            Details = $"Resolved report {request.ReportId} with action {request.Action}. Note: {request.AdminNote}"
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task BanUserLogic(User user, string reason)
    {
        user.Status = UserStatus.Banned;

        // If user has a LastLoginDeviceHash, blacklist it
        if (!string.IsNullOrEmpty(user.LastLoginDeviceHash))
        {
            // Check if already blacklisted
            var existing = await _context.BannedDevices.AnyAsync(b => b.DeviceHash == user.LastLoginDeviceHash);
            if (!existing)
            {
                _context.BannedDevices.Add(new BannedDevice
                {
                    DeviceHash = user.LastLoginDeviceHash,
                    Reason = reason,
                    BannedAt = DateTime.UtcNow
                });
            }
        }
    }
}
