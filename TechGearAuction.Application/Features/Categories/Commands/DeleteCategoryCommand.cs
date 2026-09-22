using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Categories.Commands;

public class DeleteCategoryCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand>
{
    private readonly IAppDbContext _context;

    public DeleteCategoryCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Auctions)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
            
        if (category == null)
        {
            throw new Exception("Category not found.");
        }

        if (category.SubCategories.Any())
        {
            throw new Exception("Cannot delete category because it has sub-categories. Please reassign them first.");
        }

        if (category.Auctions.Any())
        {
            throw new Exception("Cannot delete category because it has associated auctions. Please reassign them first.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
