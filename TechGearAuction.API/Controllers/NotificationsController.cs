using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _mediator.Send(new TechGearAuction.Application.Features.Notifications.Queries.GetUnreadNotificationsQuery());
        return Ok(result);
    }
}
