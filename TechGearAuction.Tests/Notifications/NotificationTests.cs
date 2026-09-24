using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Notifications.Commands;
using TechGearAuction.Application.Features.Notifications.Queries;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Notifications;

public class NotificationTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public NotificationTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task MarkNotificationRead_WhenOwner_ShouldSetIsReadTrue()
    {
        var notifId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Notifications.Add(new Notification
            {
                Id = notifId,
                UserId = TestDbFactory.UserId,
                Title = "Test",
                Content = "Content",
                IsRead = false
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new MarkNotificationReadCommandHandler(ctx, svc);

        await handler.Handle(new MarkNotificationReadCommand { NotificationId = notifId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var notif = await verifyCtx.Notifications.FindAsync(notifId);
        notif!.IsRead.Should().BeTrue();
    }

    [Fact]
    public async Task MarkNotificationRead_WhenNotOwner_ShouldThrow()
    {
        var notifId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Notifications.Add(new Notification
            {
                Id = notifId,
                UserId = TestDbFactory.UserId,
                Title = "Test",
                Content = "Content",
                IsRead = false
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id); // different user
        var ctx = _factory.CreateContext();
        var handler = new MarkNotificationReadCommandHandler(ctx, svc);

        var act = () => handler.Handle(new MarkNotificationReadCommand { NotificationId = notifId }, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("*not found or does not belong*");
    }

    [Fact]
    public async Task GetUnreadNotifications_ShouldReturnOnlyUnreadAndBelongingToUser()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Notifications.AddRange(
                new Notification { Id = Guid.NewGuid(), UserId = TestDbFactory.UserId, Title = "N1", Content = "C", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                new Notification { Id = Guid.NewGuid(), UserId = TestDbFactory.UserId, Title = "N2", Content = "C", IsRead = true, CreatedAt = DateTime.UtcNow.AddMinutes(-2) }, // read
                new Notification { Id = Guid.NewGuid(), UserId = TestDbFactory.User2Id, Title = "N3", Content = "C", IsRead = false, CreatedAt = DateTime.UtcNow.AddMinutes(-3) } // other user
            );
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new GetUnreadNotificationsQueryHandler(ctx, svc);

        var result = await handler.Handle(new GetUnreadNotificationsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result.First().Title.Should().Be("N1");
    }

    public void Dispose() => _factory.Dispose();
}
