using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TechGearAuction.Application.Features.Appeals.Commands;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/appeals")]
public class AppealsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AppealsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAppeals([FromQuery] TechGearAuction.Application.Features.Appeals.Queries.GetAppealsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("{id}/resolve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ResolveAppeal(Guid id, [FromBody] ResolveAppealCommand command)
    {
        if (id != command.AppealId) return BadRequest();
            await _mediator.Send(command);
            return Ok(new { Message = "Appeal resolved." });
    }

    public class SubmitBanAppealRequestDto
    {
        public string Email { get; set; } = null!;
        public string Description { get; set; } = null!;
        public List<IFormFile> Files { get; set; } = new();
    }

    [HttpPost("banned-users")]
    public async Task<IActionResult> SubmitBanAppeal([FromForm] SubmitBanAppealRequestDto request)
    {
            var command = new SubmitBanAppealCommand
            {
                Email = request.Email,
                Description = request.Description,
                Evidences = request.Files?.Select(f => new AppealEvidenceDto
                {
                    Stream = f.OpenReadStream(),
                    FileName = f.FileName,
                    ContentType = f.ContentType
                }).ToList() ?? new List<AppealEvidenceDto>()
            };

            var id = await _mediator.Send(command);
            return Ok(new { Message = "Appeal submitted successfully.", AppealId = id });
    }
}

