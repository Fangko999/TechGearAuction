using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Users;

public class ChangePasswordTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public ChangePasswordTests() => _factory = new TestDbFactory();

    [Fact]
    public async Task ChangePassword_WithCorrectOldPassword_ShouldSucceed()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new ChangePasswordCommandHandler(ctx, svc);

        await handler.Handle(new ChangePasswordCommand
        {
            OldPassword = "User@123",
            NewPassword = "NewSecure@456"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var user = await verifyCtx.Users.FirstAsync(u => u.Id == TestDbFactory.UserId);
        BCrypt.Net.BCrypt.Verify("NewSecure@456", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePassword_WithWrongOldPassword_ShouldThrow()
    {
        var ctx = _factory.CreateContext();
        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var handler = new ChangePasswordCommandHandler(ctx, svc);

        var act = () => handler.Handle(new ChangePasswordCommand
        {
            OldPassword = "WrongPassword!",
            NewPassword = "NewSecure@456"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    public void Dispose() => _factory.Dispose();
}
