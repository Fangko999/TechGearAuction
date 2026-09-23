using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Admin.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] GetAuditLogsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
