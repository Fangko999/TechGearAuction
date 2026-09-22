namespace TechGearAuction.Application.DTOs.User;

public class CreditTransactionDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Amount { get; set; }
    public string? Reason { get; set; }
    public Guid? AuctionId { get; set; }
    public DateTime CreatedAt { get; set; }
}
