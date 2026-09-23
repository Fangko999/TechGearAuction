using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.Features.Chat.Commands;
using TechGearAuction.Application.Features.Chat.Queries;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Chat;

public class ChatRoomTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IChatNotificationService> _notificationMock;
    private readonly Guid _chatRoomId = Guid.NewGuid();

    public ChatRoomTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IChatNotificationService>();
        SeedChatRoom();
    }

    private void SeedChatRoom()
    {
        using var ctx = _factory.CreateContext();
        ctx.ChatRooms.Add(new TechGearAuction.Domain.Entities.ChatRoom
        {
            Id = _chatRoomId,
            AuctionId = TestDbFactory.ActiveAuctionId, // Seller is UserId
            Status = ChatRoomStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddDays(30)
        });
        
        var auction = ctx.Auctions.Find(TestDbFactory.ActiveAuctionId);
        auction!.WinnerId = TestDbFactory.User2Id; // User2Id is Winner
        ctx.SaveChanges();
    }

    [Fact]
    public async Task SendMessage_WhenRoomIsActiveAndUserIsParticipant_ShouldSucceed()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId); // Seller
        var ctx = _factory.CreateContext();
        var handler = new SendMessageCommandHandler(ctx, svc, _notificationMock.Object);

        var msgId = await handler.Handle(new SendMessageCommand
        {
            ChatRoomId = _chatRoomId,
            Content = "Hello Winner!"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var msg = await verifyCtx.ChatMessages.FindAsync(msgId);
        msg.Should().NotBeNull();
        msg!.Content.Should().Be("Hello Winner!");
        
        _notificationMock.Verify(n => n.NotifyNewMessageAsync(_chatRoomId, TestDbFactory.UserId, "Hello Winner!", "Text", null, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task SendMessage_WhenRoomIsArchived_ShouldThrow()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            var r = setupCtx.ChatRooms.Find(_chatRoomId);
            r!.Status = ChatRoomStatus.Archived;
            setupCtx.SaveChanges();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new SendMessageCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new SendMessageCommand { ChatRoomId = _chatRoomId, Content = "Test" }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*is archived*");
    }

    [Fact]
    public async Task SendMessage_WhenUserIsNotParticipant_ShouldThrow()
    {
        var svc = new MockCurrentUserService(TestDbFactory.AdminId); // Random user
        var ctx = _factory.CreateContext();
        var handler = new SendMessageCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new SendMessageCommand { ChatRoomId = _chatRoomId, Content = "Test" }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*not a participant*");
    }

    [Fact]
    public async Task SendMessage_WhenUserIsBlocked_ShouldThrow()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.UserBlocks.Add(new TechGearAuction.Domain.Entities.UserBlock
            {
                BlockerId = TestDbFactory.UserId,
                BlockedId = TestDbFactory.User2Id
            });
            setupCtx.SaveChanges();
        }

        var svc = new MockCurrentUserService(TestDbFactory.User2Id); // Winner
        var ctx = _factory.CreateContext();
        var handler = new SendMessageCommandHandler(ctx, svc, _notificationMock.Object);

        var act = () => handler.Handle(new SendMessageCommand { ChatRoomId = _chatRoomId, Content = "Test" }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>().WithMessage("*because of a block*");
    }

    [Fact]
    public async Task MarkAsRead_ShouldUpdateIsReadStatusAndNotify()
    {
        // Seed unread message from User2 to User1
        Guid msgId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.ChatMessages.Add(new TechGearAuction.Domain.Entities.ChatMessage
            {
                Id = msgId,
                ChatRoomId = _chatRoomId,
                SenderId = TestDbFactory.User2Id,
                Content = "Hi",
                IsRead = false
            });
            setupCtx.SaveChanges();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId); // User1 reads
        var ctx = _factory.CreateContext();
        var handler = new MarkMessagesAsReadCommandHandler(ctx, svc, _notificationMock.Object);

        await handler.Handle(new MarkMessagesAsReadCommand { ChatRoomId = _chatRoomId }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var msg = await verifyCtx.ChatMessages.FindAsync(msgId);
        msg!.IsRead.Should().BeTrue();

        _notificationMock.Verify(n => n.NotifyMessageReadAsync(_chatRoomId, msgId), Times.Once);
    }

    [Fact]
    public async Task GetMyChatRooms_ShouldReturnRoomsWhereUserIsSellerOrWinner()
    {
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new GetMyChatRoomsQueryHandler(ctx, svc);

        var result = await handler.Handle(new GetMyChatRoomsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(_chatRoomId);
        result[0].OpponentId.Should().Be(TestDbFactory.User2Id);
    }

    [Fact]
    public async Task GetMessages_ShouldPaginateCorrectly()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            for (int i = 0; i < 15; i++)
            {
                setupCtx.ChatMessages.Add(new TechGearAuction.Domain.Entities.ChatMessage
                {
                    ChatRoomId = _chatRoomId,
                    SenderId = TestDbFactory.UserId,
                    Content = $"Msg {i}",
                    IsRead = true
                });
            }
            setupCtx.SaveChanges();
        }

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var handler = new GetChatRoomMessagesQueryHandler(ctx, svc);

        var result = await handler.Handle(new GetChatRoomMessagesQuery { ChatRoomId = _chatRoomId, PageIndex = 1, PageSize = 10 }, CancellationToken.None);

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(15);
    }

    public void Dispose() => _factory.Dispose();
}

