namespace TechGearAuction.Domain.Entities;

public class UserFollow : BaseEntity
{
    public int FollowerId { get; set; }
    public int FolloweeId { get; set; }

    public User Follower { get; set; } = null!;
    public User Followee { get; set; } = null!;
}