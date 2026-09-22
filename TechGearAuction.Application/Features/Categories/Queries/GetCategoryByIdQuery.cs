using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.Category;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Categories.Queries;

public class GetCategoryByIdQuery : IRequest<CategoryDto>
{
    public Guid Id { get; set; }
}

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    private readonly IAppDbContext _context;

    public GetCategoryByIdQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (category == null)
        {
            throw new Exception("Category not found.");
        }

        return new CategoryDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            Name = category.Name,
            Description = category.Description
        };
    }
}
