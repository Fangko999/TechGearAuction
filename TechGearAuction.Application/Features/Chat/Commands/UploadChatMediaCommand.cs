using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Chat.Commands;

public class UploadChatMediaCommand : IRequest<string>
{
    public Guid ChatRoomId { get; set; }
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class UploadChatMediaCommandHandler : IRequestHandler<UploadChatMediaCommand, string>
{
    private readonly IStorageService _storageService;
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UploadChatMediaCommandHandler(IStorageService storageService, IAppDbContext context, ICurrentUserService currentUserService)
    {
        _storageService = storageService;
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<string> Handle(UploadChatMediaCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        var chatRoom = await _context.ChatRooms
            .Include(r => r.Auction)
            .FirstOrDefaultAsync(r => r.Id == request.ChatRoomId, cancellationToken);

        if (chatRoom == null)
            throw new NotFoundException("Entity", "Chat room not found.");

        if (chatRoom.Status != ChatRoomStatus.Active)
            throw new BusinessRuleException("This chat room is not active. Media upload is disabled.");

        if (chatRoom.Auction.SellerId != userId && chatRoom.Auction.WinnerId != userId)
            throw new BusinessRuleException("You are not a participant of this chat room.");

        if (request.FileStream == null || request.FileStream.Length == 0)
            throw new BusinessRuleException("File is empty.");

        var fileName = $"{Guid.NewGuid()}_{request.FileName}";
        var url = await _storageService.UploadFileAsync(request.FileStream, fileName, request.ContentType, "chat-media");
        
        return url;
    }
}
