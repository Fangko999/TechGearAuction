using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGearAuction.Application.Features.Categories.Commands;

namespace TechGearAuction.API.Controllers;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminCategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
            var id = await _mediator.Send(command);
            return Ok(new { Message = "Category created successfully.", Id = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Message = "ID mismatch." });
        }

            await _mediator.Send(command);
            return Ok(new { Message = "Category updated successfully." });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(Guid id)
    {
            await _mediator.Send(new DeleteCategoryCommand { Id = id });
            return Ok(new { Message = "Category deleted successfully." });
    }
}

