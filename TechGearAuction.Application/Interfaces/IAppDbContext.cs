using Microsoft.EntityFrameworkCore;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<UserSocialLink> UserSocialLinks { get; set; }
    DbSet<BannedDevice> BannedDevices { get; set; }
    DbSet<CreditTransaction> CreditTransactions { get; set; }
    DbSet<Category> Categories { get; set; }
    DbSet<Auction> Auctions { get; set; }
    DbSet<AuctionImage> AuctionImages { get; set; }
    DbSet<Bid> Bids { get; set; }
    DbSet<UserFollow> UserFollows { get; set; }
    DbSet<AuctionWatch> AuctionWatches { get; set; }
    DbSet<UserBlock> UserBlocks { get; set; }
    DbSet<ChatRoom> ChatRooms { get; set; }
    DbSet<ChatMessage> ChatMessages { get; set; }
    DbSet<SuspiciousActivity> SuspiciousActivities { get; set; }
    DbSet<Report> Reports { get; set; }
    DbSet<ReportEvidence> ReportEvidences { get; set; }
    DbSet<Appeal> Appeals { get; set; }
    DbSet<AppealEvidence> AppealEvidences { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

