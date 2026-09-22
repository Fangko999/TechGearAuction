namespace TechGearAuction.Domain.Entities;

public class UserFollow : BaseEntity
{
    public Guid FollowerId { get; set; }
    public Guid FolloweeId { get; set; }

    public User Follower { get; set; } = null!;
    public User Followee { get; set; } = null!;
}
