using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Features.Users.Queries;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Users;

public class UserProfileTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public UserProfileTests() => _factory = new TestDbFactory();

    [Fact]
    public async Task GetMyProfile_ShouldReturnCorrectUserData()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new GetMyProfileQueryHandler(ctx, svc);

        var result = await handler.Handle(new GetMyProfileQuery(), CancellationToken.None);

        result.Should().NotBeNull();
        result.Email.Should().Be("user@test.com");
        result.DisplayName.Should().Be("Normal User");
    }

    [Fact]
    public async Task UpdateMyProfile_ShouldPersistNewDisplayName()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new UpdateMyProfileCommandHandler(ctx, svc);

        await handler.Handle(new UpdateMyProfileCommand { DisplayName = "Updated Name" }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var user = await verifyCtx.Users.FirstAsync(u => u.Id == TestDbFactory.UserId);
        user.DisplayName.Should().Be("Updated Name");
    }

    [Fact]
    public async Task GetPublicProfile_ShouldNotExposeEmailOrCredits()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var handler = new GetPublicProfileQueryHandler(ctx);

        var result = await handler.Handle(new GetPublicProfileQuery { UserId = TestDbFactory.UserId }, CancellationToken.None);

        result.Should().NotBeNull();
        result.DisplayName.Should().Be("Normal User");
        // Kiểm tra DTO type không có thuộc tính nhạy cảm
        var resultType = result.GetType();
        resultType.GetProperty("Email").Should().BeNull("Email must not be exposed in PublicProfileDto");
        resultType.GetProperty("AvailableCredits").Should().BeNull("Credits must not be exposed in PublicProfileDto");
        resultType.GetProperty("PasswordHash").Should().BeNull("PasswordHash must not be exposed in PublicProfileDto");
    }

    [Fact]
    public async Task GetPublicProfile_OfClosedUser_ShouldThrow()
    {
        // Close the user by setting DeletedAt
        using (var ctx = _factory.CreateContext())
        {
            var user = await ctx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.User2Id);
            user.DeletedAt = DateTime.UtcNow;
            await ctx.SaveChangesAsync();
        }

        var ctx2 = _factory.CreateContext();
        var handler = new GetPublicProfileQueryHandler(ctx2);

        var act = () => handler.Handle(new GetPublicProfileQuery { UserId = TestDbFactory.User2Id }, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>();
    }

    public void Dispose() => _factory.Dispose();
}

