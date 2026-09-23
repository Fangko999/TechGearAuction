using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Features.Auctions.Queries;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auctions;

public class SellerAuctionTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public SellerAuctionTests() => _factory = new TestDbFactory();

    // ─── Create ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAuction_ShouldCreateDraft()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateAuctionCommandHandler(ctx, svc);

        var id = await handler.Handle(new CreateAuctionCommand
        {
            CategoryId = TestDbFactory.ChildCategoryId,
            Title = "New Phone",
            StartPrice = 500,
            BidIncrement = 10,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2)
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(id);
        auction.Should().NotBeNull();
        auction!.Status.Should().Be(AuctionStatus.Draft);
    }

    [Fact]
    public async Task CreateAuction_WithBuyNowPriceLowerThanStartPrice_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateAuctionCommandHandler(ctx, svc);

        var act = () => handler.Handle(new CreateAuctionCommand
        {
            CategoryId = TestDbFactory.ChildCategoryId,
            Title = "New Phone",
            StartPrice = 500,
            BuyNowPrice = 400, // Invalid!
            BidIncrement = 10,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2)
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*greater than or equal*");
    }

    [Fact]
    public async Task CreateAuction_WithDurationLessThan3Hours_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CreateAuctionCommandHandler(ctx, svc);

        var act = () => handler.Handle(new CreateAuctionCommand
        {
            CategoryId = TestDbFactory.ChildCategoryId,
            Title = "New Phone",
            StartPrice = 500,
            BidIncrement = 10,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(2) // Only 2 hours
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*at least 3 hours*");
    }

    // ─── Cancel ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CancelAuction_WhenDraft_ShouldCancelWithoutRefund()
    {
        // Setup draft
        Guid draftId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = draftId,
                Title = "Draft",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Status = AuctionStatus.Draft,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CancelAuctionCommandHandler(ctx, svc);

        await handler.Handle(new CancelAuctionCommand { Id = draftId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(draftId);
        auction!.Status.Should().Be(AuctionStatus.Cancelled);
        
        // Ensure no refund transaction
        var refundExists = await verifyCtx.CreditTransactions.AnyAsync(t => t.UserId == TestDbFactory.UserId && t.AuctionId == draftId);
        refundExists.Should().BeFalse();
    }

    [Fact]
    public async Task CancelAuction_WhenScheduled_ShouldCancelAndRefundCreditAndLogLedger()
    {
        // Setup scheduled
        Guid scheduledId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = scheduledId,
                Title = "Scheduled",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Status = AuctionStatus.Scheduled,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
            });
            // Make sure user has initial credits
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CancelAuctionCommandHandler(ctx, svc);

        await handler.Handle(new CancelAuctionCommand { Id = scheduledId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(scheduledId);
        auction!.Status.Should().Be(AuctionStatus.Cancelled);
        
        var user = await verifyCtx.Users.FindAsync(TestDbFactory.UserId);
        user!.AvailableCredits.Should().Be(6); // 5 initial + 1 refund

        var refundExists = await verifyCtx.CreditTransactions.AnyAsync(t => t.UserId == TestDbFactory.UserId && t.AuctionId == scheduledId && t.Amount == 1);
        refundExists.Should().BeTrue("Ledger must be updated on refund");
    }

    [Fact]
    public async Task CancelAuction_WhenActive_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new CancelAuctionCommandHandler(ctx, svc);

        var act = () => handler.Handle(new CancelAuctionCommand { Id = TestDbFactory.ActiveAuctionId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Active auctions cannot be cancelled*");
    }

    // ─── Publish ────────────────────────────────────────────────────────────

    [Fact]
    public async Task PublishAuction_WithValidData_ShouldDeductCreditAndSetScheduled()
    {
        // Setup draft with image
        Guid draftId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = draftId,
                Title = "Ready to publish",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(1), // Future
                EndTime = DateTime.UtcNow.AddDays(2),
                Status = AuctionStatus.Draft,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId,
                Images = new List<TechGearAuction.Domain.Entities.AuctionImage>
                {
                    new TechGearAuction.Domain.Entities.AuctionImage { Id = Guid.NewGuid(), ImageUrl = "http://test.com/img.jpg" }
                }
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new PublishAuctionCommandHandler(ctx, svc);

        await handler.Handle(new PublishAuctionCommand { AuctionId = draftId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var auction = await verifyCtx.Auctions.FindAsync(draftId);
        auction!.Status.Should().Be(AuctionStatus.Scheduled);

        var user = await verifyCtx.Users.FindAsync(TestDbFactory.UserId);
        user!.AvailableCredits.Should().Be(4); // 5 initial - 1 cost

        var deductExists = await verifyCtx.CreditTransactions.AnyAsync(t => t.UserId == TestDbFactory.UserId && t.AuctionId == draftId && t.Amount == -1);
        deductExists.Should().BeTrue("Ledger must be updated on publish");
    }

    [Fact]
    public async Task PublishAuction_WithNoImages_ShouldThrow()
    {
        Guid draftId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Auctions.Add(new TechGearAuction.Domain.Entities.Auction
            {
                Id = draftId,
                Title = "Ready to publish",
                StartPrice = 10,
                BidIncrement = 1,
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(2),
                Status = AuctionStatus.Draft,
                SellerId = TestDbFactory.UserId,
                CategoryId = TestDbFactory.ChildCategoryId
                // No images!
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new PublishAuctionCommandHandler(ctx, svc);

        var act = () => handler.Handle(new PublishAuctionCommand { AuctionId = draftId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*least one image*");
    }

    public void Dispose() => _factory.Dispose();
}

