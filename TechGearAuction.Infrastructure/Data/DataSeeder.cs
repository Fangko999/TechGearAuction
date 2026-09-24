using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using BCrypt.Net;

namespace TechGearAuction.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedDataAsync(AppDbContext context)
    {
        // 1. Check if DB is already seeded
        if (await context.Users.AnyAsync())
        {
            return; // DB has been seeded
        }

        // 2. Seed Users
        var defaultPassword = BCrypt.Net.BCrypt.HashPassword("Password123!");
        
        var admin = new User { IsEmailVerified = true, Id = Guid.NewGuid(), Email = "admin@techgear.com", PasswordHash = defaultPassword, Role = UserRole.Admin, DisplayName = "Super Admin" };
        var seller1 = new User { IsEmailVerified = true, Id = Guid.NewGuid(), Email = "seller1@techgear.com", PasswordHash = defaultPassword, Role = UserRole.User, DisplayName = "Gear Store VN", AverageRating = 4.8, TotalReviews = 120 };
        var seller2 = new User { IsEmailVerified = true, Id = Guid.NewGuid(), Email = "seller2@techgear.com", PasswordHash = defaultPassword, Role = UserRole.User, DisplayName = "Flashlight Pro", AverageRating = 4.5, TotalReviews = 45 };
        var buyer1 = new User { IsEmailVerified = true, Id = Guid.NewGuid(), Email = "buyer1@techgear.com", PasswordHash = defaultPassword, Role = UserRole.User, DisplayName = "Nguyễn Văn Mua" };
        var buyer2 = new User { IsEmailVerified = true, Id = Guid.NewGuid(), Email = "buyer2@techgear.com", PasswordHash = defaultPassword, Role = UserRole.User, DisplayName = "Trần Thị Bid" };
        var cheater = new User { IsEmailVerified = true, Id = Guid.NewGuid(), Email = "cheater@techgear.com", PasswordHash = defaultPassword, Role = UserRole.User, DisplayName = "Scammer" };

        await context.Users.AddRangeAsync(admin, seller1, seller2, buyer1, buyer2, cheater);

        // 3. Seed Categories
        var catEDC = new Category { Id = Guid.Parse("d3b07384-d9a7-4b7b-b35f-155e99859f51"), Name = "Đồ chơi EDC", Description = "Everyday Carry tools" };
        var catFlashlight = new Category { Id = Guid.Parse("d3b07384-d9a7-4b7b-b35f-155e99859f52"), Name = "Đèn pin siêu sáng", Description = "Đèn pin các loại" };
        var catBackpack = new Category { Id = Guid.Parse("d3b07384-d9a7-4b7b-b35f-155e99859f53"), Name = "Balo & Túi", Description = "Balo dã ngoại, túi chiến thuật" };

        await context.Categories.AddRangeAsync(catEDC, catFlashlight, catBackpack);

        // 4. Seed Auctions
        var auction1 = new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = seller1.Id,
            CategoryId = catEDC.Id,
            Title = "Dao đa năng Leatherman Wave+",
            Description = "Hàng chính hãng, mới 99%. Đầy đủ phụ kiện bao da.",
            StartPrice = 1500000,
            CurrentPrice = 1800000, // Đã có người bid
            BidIncrement = 50000,
            BuyNowPrice = 2500000,
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(2),
            Status = AuctionStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var auction2 = new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = seller2.Id,
            CategoryId = catFlashlight.Id,
            Title = "Đèn pin Olight Baton 3 Pro",
            Description = "Độ sáng 1500 lumens. Pin sạc nam châm.",
            StartPrice = 800000,
            CurrentPrice = 800000,
            BidIncrement = 20000,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(12),
            Status = AuctionStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        var auction3 = new Auction
        {
            Id = Guid.NewGuid(),
            SellerId = seller1.Id,
            CategoryId = catBackpack.Id,
            Title = "Balo 5.11 Tactical RUSH12",
            Description = "Màu sa mạc (Coyote). Đã sử dụng 1 lần đi phượt.",
            StartPrice = 1200000,
            CurrentPrice = 2100000,
            BidIncrement = 100000,
            StartTime = DateTime.UtcNow.AddDays(-5),
            EndTime = DateTime.UtcNow.AddDays(-1), // Đã kết thúc
            Status = AuctionStatus.Completed,
            WinnerId = buyer1.Id, // buyer1 won
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };

        await context.Auctions.AddRangeAsync(auction1, auction2, auction3);

        // Images for Auctions
        await context.AuctionImages.AddRangeAsync(
            new AuctionImage { AuctionId = auction1.Id, ImageUrl = "https://picsum.photos/id/1/400/400", IsPrimary = true },
            new AuctionImage { AuctionId = auction2.Id, ImageUrl = "https://picsum.photos/id/2/400/400", IsPrimary = true },
            new AuctionImage { AuctionId = auction3.Id, ImageUrl = "https://picsum.photos/id/3/400/400", IsPrimary = true }
        );

        // 5. Seed Bids
        var bid1 = new Bid { AuctionId = auction1.Id, BidderId = buyer1.Id, BidAmount = 1600000, CreatedAt = DateTime.UtcNow.AddHours(-10), IpAddress = "192.168.1.1", DeviceHash = "dev1" };
        var bid2 = new Bid { AuctionId = auction1.Id, BidderId = buyer2.Id, BidAmount = 1750000, CreatedAt = DateTime.UtcNow.AddHours(-8), IpAddress = "192.168.1.2", DeviceHash = "dev2" };
        var bid3 = new Bid { AuctionId = auction1.Id, BidderId = buyer1.Id, BidAmount = 1800000, CreatedAt = DateTime.UtcNow.AddHours(-2), IpAddress = "192.168.1.1", DeviceHash = "dev1" };
        
        var bid4 = new Bid { AuctionId = auction3.Id, BidderId = buyer1.Id, BidAmount = 2100000, CreatedAt = DateTime.UtcNow.AddDays(-2), IpAddress = "192.168.1.1", DeviceHash = "dev1" };

        await context.Bids.AddRangeAsync(bid1, bid2, bid3, bid4);

        // 6. Seed Chat Room (For the ended auction3 between seller1 and buyer1)
        var chatRoom = new ChatRoom
        {
            Id = Guid.NewGuid(),
            AuctionId = auction3.Id,
            Status = ChatRoomStatus.Active,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(13) // 14 days after auction ends
        };
        await context.ChatRooms.AddAsync(chatRoom);

        // Chat Messages
        var msg1 = new ChatMessage { ChatRoomId = chatRoom.Id, SenderId = seller1.Id, Content = "Chào bạn, chúc mừng bạn đã thắng đấu giá. Vui lòng gửi địa chỉ để mình ship hàng.", MessageType = ChatMessageType.Text, CreatedAt = DateTime.UtcNow.AddHours(-23), IsRead = true };
        var msg2 = new ChatMessage { ChatRoomId = chatRoom.Id, SenderId = buyer1.Id, Content = "Ok shop. Địa chỉ của mình là 123 Lê Lợi, Q1.", MessageType = ChatMessageType.Text, CreatedAt = DateTime.UtcNow.AddHours(-22), IsRead = true };
        
        await context.ChatMessages.AddRangeAsync(msg1, msg2);

        // 7. Seed Report (Dispute) for auction3
        var report = new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = buyer1.Id,
            ReportedUserId = cheater.Id,
            AuctionId = auction3.Id,
            ChatRoomId = chatRoom.Id,
            Type = ReportType.Scam,
            Description = "Tôi vừa khui hộp thì thấy balo bị rách ở đáy. Nhắn tin seller không trả lời.",
            Status = ReportStatus.Pending,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        };
        await context.Reports.AddAsync(report);

        // Report Evidence
        var evidence = new ReportEvidence
        {
            ReportId = report.Id,
            MediaUrl = "https://picsum.photos/id/11/400/400", // Fake image URL
            Type = MediaType.Image
        };
        await context.ReportEvidences.AddAsync(evidence);

        // 8. Commit
        await context.SaveChangesAsync();
    }
}

