using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserSocialLink> UserSocialLinks { get; set; }
    public DbSet<BannedDevice> BannedDevices { get; set; }
    public DbSet<CreditTransaction> CreditTransactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Auction> Auctions { get; set; }
    public DbSet<AuctionImage> AuctionImages { get; set; }
    public DbSet<Bid> Bids { get; set; }
    public DbSet<UserFollow> UserFollows { get; set; }
    public DbSet<AuctionWatch> AuctionWatches { get; set; }
    public DbSet<UserBlock> UserBlocks { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<SuspiciousActivity> SuspiciousActivities { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<ReportEvidence> ReportEvidences { get; set; }
    public DbSet<Appeal> Appeals { get; set; }
    public DbSet<AppealEvidence> AppealEvidences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Primary Keys & Concurrency
        modelBuilder.Entity<BannedDevice>().HasKey(bd => bd.DeviceHash);
        modelBuilder.Entity<Auction>().Property(a => a.RowVersion).IsRowVersion();

        // 2. Chống Spam & Race Condition (Partial Unique Indexes)
        modelBuilder.Entity<Bid>()
            .HasIndex(b => new { b.AuctionId, b.BidAmount }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<UserFollow>()
            .HasIndex(uf => new { uf.FollowerId, uf.FolloweeId }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<AuctionWatch>()
            .HasIndex(aw => new { aw.UserId, aw.AuctionId }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<UserBlock>()
            .HasIndex(ub => new { ub.BlockerId, ub.BlockedId }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        // 3. Unique Constraints (1-1 Relationships)
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

        modelBuilder.Entity<ChatRoom>()
            .HasIndex(c => c.AuctionId).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<Appeal>()
            .HasIndex(a => a.ReportId).IsUnique().HasFilter("[DeletedAt] IS NULL");

        // 4. Check Constraints & Decimals
        modelBuilder.Entity<Auction>()
            .HasCheckConstraint("CK_Auction_BuyNowPrice", "[BuyNowPrice] IS NULL OR [BuyNowPrice] >= [StartPrice]");

        modelBuilder.Entity<Auction>().Property(a => a.StartPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Auction>().Property(a => a.CurrentPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Auction>().Property(a => a.BidIncrement).HasPrecision(18, 2);
        modelBuilder.Entity<Auction>().Property(a => a.BuyNowPrice).HasPrecision(18, 2);
        modelBuilder.Entity<Bid>().Property(b => b.BidAmount).HasPrecision(18, 2);

        // 5. Cấu hình độ dài chuỗi
        modelBuilder.Entity<User>().Property(u => u.PasswordHash).HasMaxLength(255);

        // 6. Restrict Cascade Deletes (Tránh lỗi Multiple Cascade Paths trong SQL Server)
        modelBuilder.Entity<Category>()
            .HasOne(c => c.Parent).WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Auction>()
            .HasOne(a => a.Seller).WithMany().HasForeignKey(a => a.SellerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Auction>()
            .HasOne(a => a.Winner).WithMany().HasForeignKey(a => a.WinnerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Bid>()
            .HasOne(b => b.Bidder).WithMany().HasForeignKey(b => b.BidderId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.Reporter).WithMany().HasForeignKey(r => r.ReporterId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(r => r.ReportedUser).WithMany().HasForeignKey(r => r.ReportedUserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Follower).WithMany().HasForeignKey(uf => uf.FollowerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Followee).WithMany().HasForeignKey(uf => uf.FolloweeId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.Blocker).WithMany().HasForeignKey(ub => ub.BlockerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.Blocked).WithMany().HasForeignKey(ub => ub.BlockedId).OnDelete(DeleteBehavior.Restrict);

        // 7. Global Query Filters (Tự động ẩn bản ghi bị xóa mềm)
        modelBuilder.Entity<User>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<UserSocialLink>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Auction>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AuctionImage>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Bid>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<UserFollow>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AuctionWatch>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<UserBlock>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ChatRoom>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Report>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ReportEvidence>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Appeal>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AppealEvidence>().HasQueryFilter(e => e.DeletedAt == null);
    }

    // 8. Tự động hóa Audit Log & Soft Delete
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.DeletedAt = DateTime.UtcNow;
                    break;
            }
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}