using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Category;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Categories.Queries;

public class GetCategoriesQuery : IRequest<List<CategoryDto>>
{
}

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IAppDbContext _context;

    public GetCategoriesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        // Build a tree structure
        var categoryDtos = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            ParentId = c.ParentId,
            Name = c.Name,
            Description = c.Description
        }).ToList();

        var rootCategories = categoryDtos.Where(c => c.ParentId == null).ToList();

        foreach (var root in rootCategories)
        {
            BuildCategoryTree(root, categoryDtos);
        }

        return rootCategories;
    }

    private void BuildCategoryTree(CategoryDto current, List<CategoryDto> allCategories)
    {
        var children = allCategories.Where(c => c.ParentId == current.Id).ToList();
        current.SubCategories = children;
        foreach (var child in children)
        {
            BuildCategoryTree(child, allCategories);
        }
    }
}

