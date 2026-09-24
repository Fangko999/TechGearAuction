using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.DTOs.Auth;
using TechGearAuction.Application.Features.Auctions.Commands;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.DataBoundary;

public class DataBoundaryTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IAuctionNotificationService> _notificationMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IJwtProvider> _jwtProviderMock;
    private readonly Mock<IEmailService> _emailServiceMock;

    public DataBoundaryTests()
    {
        _factory = new TestDbFactory();
        _notificationMock = new Mock<IAuctionNotificationService>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _currentUserMock.Setup(m => m.UserId).Returns(TestDbFactory.UserId);
        
        _jwtProviderMock = new Mock<IJwtProvider>();
        _emailServiceMock = new Mock<IEmailService>();
    }

    [Fact]
    public async Task CreateAuction_WithTitleExceeding200Chars_ShouldThrow()
    {
        using var ctx = _factory.CreateContext();
        var handler = new CreateAuctionCommandHandler(ctx, _currentUserMock.Object);

        var command = new CreateAuctionCommand
        {
            CategoryId = TestDbFactory.ChildCategoryId,
            Title = new string('A', 201), // Exceeds 200 chars
            StartPrice = 100,
            BidIncrement = 10,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(2)
        };

        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>().WithMessage("*200*");
    }

    [Fact]
    public async Task PlaceBid_WithDecimalAmount_ShouldHandleCorrectly()
    {
        using var ctx = _factory.CreateContext();
        _currentUserMock.Setup(m => m.UserId).Returns(TestDbFactory.User2Id);
        var handler = new PlaceBidCommandHandler(ctx, _currentUserMock.Object, _notificationMock.Object);

        var command = new PlaceBidCommand
        {
            AuctionId = TestDbFactory.ActiveAuctionId,
            BidAmount = 1500.5m, // Fractional decimal
            IpAddress = "127.0.0.1",
            DeviceHash = "test"
        };

        // Should not throw, should successfully handle decimal
        await handler.Handle(command, CancellationToken.None);

        var bid = await ctx.Bids.FirstOrDefaultAsync(b => b.BidAmount == 1500.5m);
        bid.Should().NotBeNull();
        bid!.BidAmount.Should().Be(1500.5m);
    }

    [Fact]
    public void DepositCredit_WithMaxDecimalValue_ShouldThrow()
    {
        // Simulate an API receiving a massive decimal for an int property
        var json = $$"""
        {
            "Amount": {{decimal.MaxValue}}
        }
        """;

        Action act = () => JsonSerializer.Deserialize<DepositCreditCommand>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        
        // Deserializing a max decimal to int should throw JsonException
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public async Task Register_WithEmailExceeding256Chars_ShouldThrow()
    {
        using var ctx = _factory.CreateContext();
        var authService = new AuthService(ctx, _jwtProviderMock.Object, _emailServiceMock.Object);

        var dto = new RegisterDto
        {
            Email = new string('a', 250) + "@example.com", // Total > 256
            Password = "Password123!",
            DisplayName = "Test"
        };

        Func<Task> act = async () => await authService.RegisterAsync(dto);
        
        // Expecting an exception regarding email length
        await act.Should().ThrowAsync<Exception>().WithMessage("*256*");
    }

    public void Dispose()
    {
        try { _factory.Dispose(); } catch { }
    }
}
