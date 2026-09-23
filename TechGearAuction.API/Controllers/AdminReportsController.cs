using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Reports.Commands;
using TechGearAuction.Application.Features.Reports.Queries;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/reports")]
[Authorize(Roles = "Admin")]
public class AdminReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports([FromQuery] GetReportsQuery query)
    {
        try
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReportById(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetReportByIdQuery { Id = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return NotFound(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> ResolveReport(Guid id, [FromBody] ResolveReportCommand command)
    {
        if (id != command.ReportId)
            return BadRequest(new { Message = "ID mismatch." });

        try
        {
            await _mediator.Send(command);
            return Ok(new { Message = "Report resolved successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("{id}/chat-history")]
    public async Task<IActionResult> GetChatHistory(Guid id)
    {
        try
        {
            var result = await _mediator.Send(new GetAdminReportChatHistoryQuery { ReportId = id });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }
}

