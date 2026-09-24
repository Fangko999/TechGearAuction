using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Categories.Commands;

public class UpdateCategoryCommand : IRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public Guid? ParentId { get; set; }
}

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand>
{
    private readonly IAppDbContext _context;

    public UpdateCategoryCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Entity", "Category not found.");
        }

        if (request.ParentId == request.Id)
        {
            throw new ArgumentException("A category cannot be its own parent.");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.ParentId = request.ParentId;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

