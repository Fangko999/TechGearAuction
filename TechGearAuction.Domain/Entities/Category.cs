namespace TechGearAuction.Domain.Entities;

public class Category : BaseEntity
{
    public int? ParentId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public Category? Parent { get; set; }
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    public ICollection<Auction> Auctions { get; set; } = new List<Auction>();
}