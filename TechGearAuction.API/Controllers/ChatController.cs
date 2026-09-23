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
    public async Task<IActionResult> GetMyChatRooms()
    {
        try
        {
            var result = await _mediator.Send(new GetMyChatRoomsQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{roomId}/messages")]
    public async Task<IActionResult> GetMessages(Guid roomId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 50)
    {
        try
        {
            var result = await _mediator.Send(new GetChatRoomMessagesQuery 
            { 
                ChatRoomId = roomId, 
                PageIndex = pageIndex, 
                PageSize = pageSize 
            });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPost("{roomId}/messages")]
    public async Task<IActionResult> SendMessage(Guid roomId, [FromBody] SendMessageDto dto)
    {
        try
        {
            var result = await _mediator.Send(new SendMessageCommand 
            { 
                ChatRoomId = roomId, 
                Content = dto.Content 
            });
            return Ok(new { MessageId = result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{roomId}/messages/read")]
    public async Task<IActionResult> MarkAsRead(Guid roomId)
    {
        try
        {
            await _mediator.Send(new MarkMessagesAsReadCommand { ChatRoomId = roomId });
            return Ok(new { Message = "Messages marked as read." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}
