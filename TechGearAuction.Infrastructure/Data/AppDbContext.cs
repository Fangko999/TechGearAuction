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
    public DbSet<AdminAuditLog> AdminAuditLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserDeviceLog> UserDeviceLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations from assembly (for User, Auction, Bid, ChatRoom, Report, Appeal)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Remaining Entity Configurations
        modelBuilder.Entity<BannedDevice>().HasKey(bd => bd.DeviceHash);

        modelBuilder.Entity<UserFollow>()
            .HasIndex(uf => new { uf.FollowerId, uf.FolloweeId }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<AuctionWatch>()
            .HasIndex(aw => new { aw.UserId, aw.AuctionId }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<UserBlock>()
            .HasIndex(ub => new { ub.BlockerId, ub.BlockedId }).IsUnique().HasFilter("[DeletedAt] IS NULL");

        modelBuilder.Entity<Category>()
            .HasOne(c => c.Parent).WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Follower).WithMany().HasForeignKey(uf => uf.FollowerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserFollow>()
            .HasOne(uf => uf.Followee).WithMany().HasForeignKey(uf => uf.FolloweeId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.Blocker).WithMany().HasForeignKey(ub => ub.BlockerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserBlock>()
            .HasOne(ub => ub.Blocked).WithMany().HasForeignKey(ub => ub.BlockedId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AdminAuditLog>()
            .HasOne(l => l.Admin).WithMany().HasForeignKey(l => l.AdminId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<UserDeviceLog>()
            .HasOne<User>().WithMany().HasForeignKey(u => u.UserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CreditTransaction>()
            .HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CreditTransaction>()
            .HasOne(c => c.Auction).WithMany().HasForeignKey(c => c.AuctionId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SuspiciousActivity>()
            .HasOne(s => s.Bidder).WithMany().HasForeignKey(s => s.BidderId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SuspiciousActivity>()
            .HasOne(s => s.Seller).WithMany().HasForeignKey(s => s.SellerId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<SuspiciousActivity>()
            .HasOne(s => s.Auction).WithMany().HasForeignKey(s => s.AuctionId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuctionWatch>()
            .HasOne(w => w.User).WithMany().HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AuctionWatch>()
            .HasOne(w => w.Auction).WithMany().HasForeignKey(w => w.AuctionId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ChatMessage>()
            .HasOne(m => m.Sender).WithMany().HasForeignKey(m => m.SenderId).OnDelete(DeleteBehavior.Restrict).IsRequired(false);

        // Global Query Filters
        modelBuilder.Entity<UserSocialLink>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AuctionImage>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<UserFollow>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AuctionWatch>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<UserBlock>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ChatMessage>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<ReportEvidence>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<AppealEvidence>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<CreditTransaction>().HasQueryFilter(e => e.DeletedAt == null);
        modelBuilder.Entity<SuspiciousActivity>().HasQueryFilter(e => e.DeletedAt == null);
    }

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