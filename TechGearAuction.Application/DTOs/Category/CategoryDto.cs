namespace TechGearAuction.Application.DTOs.Category;

public record CategoryDto
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    
    // For nested tree structure
    public List<CategoryDto> SubCategories { get; set; } = new List<CategoryDto>();
}


