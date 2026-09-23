using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Features.Auctions.Queries;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auctions;

public class BuyerAuctionTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public BuyerAuctionTests() => _factory = new TestDbFactory();

    // ─── Queries ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAuctions_ShouldFilterByStatusAndCategory()
    {
        var ctx = _factory.CreateContext();
        var handler = new GetAuctionsQueryHandler(ctx);

        var result = await handler.Handle(new GetAuctionsQuery
        {
            CategoryId = TestDbFactory.ChildCategoryId,
            Status = AuctionStatus.Active,
            PageIndex = 1,
            PageSize = 10
        }, CancellationToken.None);

        result.Items.Should().NotBeEmpty();
        result.Items.All(a => a.Status == AuctionStatus.Active.ToString()).Should().BeTrue();
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnDetails()
    {
        var ctx = _factory.CreateContext();
        var handler = new GetAuctionByIdQueryHandler(ctx);

        var result = await handler.Handle(new GetAuctionByIdQuery { Id = TestDbFactory.ActiveAuctionId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(TestDbFactory.ActiveAuctionId);
        result.Title.Should().Be("MacBook Pro");
    }

    // ─── Watchlist ──────────────────────────────────────────────────────────

    [Fact]
    public async Task ToggleWatch_ShouldAddOnFirstCall()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id); // User2 watching User1's auction
        var ctx = _factory.CreateContext();
        var handler = new ToggleAuctionWatchCommandHandler(ctx, svc);

        var isWatching = await handler.Handle(new ToggleAuctionWatchCommand { AuctionId = TestDbFactory.ActiveAuctionId }, CancellationToken.None);

        isWatching.Should().BeTrue();

        using var verifyCtx = _factory.CreateContext();
        var watchExists = await verifyCtx.AuctionWatches.AnyAsync(w => w.UserId == TestDbFactory.User2Id && w.AuctionId == TestDbFactory.ActiveAuctionId);
        watchExists.Should().BeTrue();
    }

    [Fact]
    public async Task ToggleWatch_ShouldRemoveOnSecondCall()
    {
        var svc = new MockCurrentUserService(TestDbFactory.User2Id);
        
        // Add watch
        await new ToggleAuctionWatchCommandHandler(_factory.CreateContext(), svc)
            .Handle(new ToggleAuctionWatchCommand { AuctionId = TestDbFactory.ActiveAuctionId }, CancellationToken.None);

        // Remove watch
        var ctx = _factory.CreateContext();
        var handler = new ToggleAuctionWatchCommandHandler(ctx, svc);
        var isWatching = await handler.Handle(new ToggleAuctionWatchCommand { AuctionId = TestDbFactory.ActiveAuctionId }, CancellationToken.None);

        isWatching.Should().BeFalse();
        
        using var verifyCtx = _factory.CreateContext();
        var watchExists = await verifyCtx.AuctionWatches.AnyAsync(w => w.UserId == TestDbFactory.User2Id && w.AuctionId == TestDbFactory.ActiveAuctionId);
        watchExists.Should().BeFalse();
    }

    [Fact]
    public async Task ToggleWatch_OnOwnAuction_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId); // Seller
        var ctx = _factory.CreateContext();
        var handler = new ToggleAuctionWatchCommandHandler(ctx, svc);

        var act = () => handler.Handle(new ToggleAuctionWatchCommand { AuctionId = TestDbFactory.ActiveAuctionId }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>().WithMessage("*own auction*");
    }

    public void Dispose() => _factory.Dispose();
}
