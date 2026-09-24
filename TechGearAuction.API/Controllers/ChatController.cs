using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.DTOs.Chat;
using TechGearAuction.Application.Features.Chat.Commands;
using TechGearAuction.Application.Features.Chat.Queries;

namespace TechGearAuction.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyChatRooms([FromQuery] string role = "All", [FromQuery] string folder = "Inbox")
    {
            var result = await _mediator.Send(new GetMyChatRoomsQuery { Role = role, Folder = folder });
            return Ok(result);
    }

    [HttpGet("{roomId}/messages")]
    public async Task<IActionResult> GetMessages(Guid roomId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
    {
            var result = await _mediator.Send(new GetChatRoomMessagesQuery 
            { 
                ChatRoomId = roomId, 
                PageIndex = pageIndex, 
                PageSize = pageSize 
            });
            return Ok(result);
    }

    [HttpPost("{roomId}/messages")]
    public async Task<IActionResult> SendMessage(Guid roomId, [FromBody] SendMessageDto dto)
    {
            var result = await _mediator.Send(new SendMessageCommand 
            { 
                ChatRoomId = roomId, 
                Content = dto.Content,
                MessageType = dto.MessageType,
                MediaUrl = dto.MediaUrl
            });
            return Ok(new { MessageId = result });
    }

    [HttpPut("{roomId}/messages/read")]
    public async Task<IActionResult> MarkAsRead(Guid roomId)
    {
            await _mediator.Send(new MarkMessagesAsReadCommand { ChatRoomId = roomId });
            return Ok(new { Message = "Messages marked as read." });
    }

    [HttpPut("{roomId}/archive")]
    public async Task<IActionResult> ArchiveChatRoom(Guid roomId)
    {
            await _mediator.Send(new ArchiveChatRoomCommand { ChatRoomId = roomId });
            return Ok(new { Message = "Chat room archived successfully." });
    }

    [HttpPost("{roomId}/media")]
    public async Task<IActionResult> UploadMedia(Guid roomId, IFormFile file)
    {
            if (file == null || file.Length == 0)
                return BadRequest(new { Message = "File is missing." });

            using var stream = file.OpenReadStream();
            var url = await _mediator.Send(new UploadChatMediaCommand 
            { 
                ChatRoomId = roomId,
                FileStream = stream,
                FileName = file.FileName,
                ContentType = file.ContentType
            });
            return Ok(new { Url = url });
    }
}
