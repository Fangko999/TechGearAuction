using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Chat;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Reports.Queries;

public class GetAdminReportChatHistoryQuery : IRequest<List<ChatMessageDto>>
{
    public Guid ReportId { get; set; }
}

public class GetAdminReportChatHistoryQueryHandler : IRequestHandler<GetAdminReportChatHistoryQuery, List<ChatMessageDto>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public GetAdminReportChatHistoryQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<List<ChatMessageDto>> Handle(GetAdminReportChatHistoryQuery request, CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var report = await _context.Reports.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken);

        if (report == null)
            throw new NotFoundException("Entity", "Report not found.");

        if (report.ChatRoomId == null)
            throw new BusinessRuleException("This report is not linked to any chat room.");

        var messages = await _context.ChatMessages.AsNoTracking()
            .Include(m => m.Sender)
            .Where(m => m.ChatRoomId == report.ChatRoomId.Value)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        _context.AdminAuditLogs.Add(new AdminAuditLog
        {
            AdminId = adminId,
            Action = "ViewReportChatHistory",
            EntityType = "Report",
            EntityId = request.ReportId,
            Details = $"Admin {adminId} viewed chat history for Report {request.ReportId}"
        });

        await _context.SaveChangesAsync(cancellationToken);

        return messages.Select(m => new ChatMessageDto
        {
            Id = m.Id,
            ChatRoomId = m.ChatRoomId,
            SenderId = m.SenderId,
            SenderName = m.Sender?.DisplayName,
            Content = m.Content,
            MessageType = m.MessageType.ToString(),
            MediaUrl = m.MediaUrl,
            IsRead = m.IsRead,
            CreatedAt = m.CreatedAt
        }).ToList();
    }
}
