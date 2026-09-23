using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Features.Users.Queries;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Users;

public class CreditTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public CreditTests() => _factory = new TestDbFactory();

    [Fact]
    public async Task DepositCredit_WithPositiveAmount_ShouldUpdateCreditsAndCreateTransaction()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new DepositCreditCommandHandler(ctx, svc);

        await handler.Handle(new DepositCreditCommand { Amount = 10 }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var user = await verifyCtx.Users.FirstAsync(u => u.Id == TestDbFactory.UserId);
        user.AvailableCredits.Should().Be(15); // started with 5

        var txExists = await verifyCtx.CreditTransactions
            .AnyAsync(t => t.UserId == TestDbFactory.UserId && t.Amount == 10 && t.Reason == "Nạp Credit hệ thống");
        txExists.Should().BeTrue();
    }

    [Fact]
    public async Task DepositCredit_WithZeroAmount_ShouldThrow()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new DepositCreditCommandHandler(ctx, svc);

        var act = () => handler.Handle(new DepositCreditCommand { Amount = 0 }, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task DepositCredit_WithNegativeAmount_ShouldThrow()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new DepositCreditCommandHandler(ctx, svc);

        var act = () => handler.Handle(new DepositCreditCommand { Amount = -5 }, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCreditHistory_ShouldReturnListOrderedByDateDesc()
    {
        // Add a second transaction (newer)
        using (var ctx = _factory.CreateContext())
        {
            ctx.CreditTransactions.Add(new TechGearAuction.Domain.Entities.CreditTransaction
            {
                UserId = TestDbFactory.UserId,
                Amount = 5,
                Reason = "Newer Deposit",
                CreatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var ctx2 = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new GetMyCreditHistoryQueryHandler(ctx2, svc);

        var result = await handler.Handle(new GetMyCreditHistoryQuery(), CancellationToken.None);

        result.Should().HaveCountGreaterThan(1);
        result.First().Reason.Should().Be("Newer Deposit", "most recent transaction should come first");
    }

    public void Dispose() => _factory.Dispose();
}

