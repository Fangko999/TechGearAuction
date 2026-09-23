using FluentAssertions;
using TechGearAuction.Application.Features.SuspiciousActivities.Commands;
using TechGearAuction.Application.Features.SuspiciousActivities.Queries;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Admin;

public class SuspiciousActivitiesTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public SuspiciousActivitiesTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task GetActivities_ShouldReturnList()
    {
        // Arrange
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.SuspiciousActivities.Add(new SuspiciousActivity
            {
                AuctionId = TestDbFactory.ActiveAuctionId,
                BidderId = TestDbFactory.User2Id,
                SellerId = TestDbFactory.UserId,
                Reason = "Test",
                IsReviewed = false
            });
            await setupCtx.SaveChangesAsync();
        }

        var handler = new GetSuspiciousActivitiesQueryHandler(_factory.CreateContext());

        // Act
        var result = await handler.Handle(new GetSuspiciousActivitiesQuery(), CancellationToken.None);

        // Assert
        result.Items.Should().NotBeEmpty();
        result.Items[0].Reason.Should().Be("Test");
        result.Items[0].IsReviewed.Should().BeFalse();
    }

    [Fact]
    public async Task ReviewActivity_ShouldMarkAsReviewed()
    {
        // Arrange
        var activityId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.SuspiciousActivities.Add(new SuspiciousActivity
            {
                Id = activityId,
                Reason = "To review",
                IsReviewed = false
            });
            await setupCtx.SaveChangesAsync();
        }

        var handler = new ReviewSuspiciousActivityCommandHandler(_factory.CreateContext());

        // Act
        await handler.Handle(new ReviewSuspiciousActivityCommand { Id = activityId }, CancellationToken.None);

        // Assert
        using var verifyCtx = _factory.CreateContext();
        var act = await verifyCtx.SuspiciousActivities.FindAsync(activityId);
        act!.IsReviewed.Should().BeTrue();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}

