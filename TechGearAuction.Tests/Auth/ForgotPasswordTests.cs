using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.DTOs.Auth;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auth;

public class ForgotPasswordTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IJwtProvider> _jwtMock;

    public ForgotPasswordTests()
    {
        _factory = new TestDbFactory();
        _emailMock = new Mock<IEmailService>();
        _jwtMock = new Mock<IJwtProvider>();
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");
    }

    private AuthService CreateService() =>
        new AuthService(_factory.CreateContext(), _jwtMock.Object, _emailMock.Object);

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldGenerateTokenAndExpiry()
    {
        var dto = new ForgotPasswordDto { Email = "user@test.com" };

        await CreateService().ForgotPasswordAsync(dto);

        using var ctx = _factory.CreateContext();
        var user = await ctx.Users.IgnoreQueryFilters().FirstAsync(u => u.Email == "user@test.com");
        user.PasswordResetToken.Should().NotBeNullOrEmpty();
        user.PasswordResetTokenExpiry.Should().BeAfter(DateTime.UtcNow);

        _emailMock.Verify(e => e.SendEmailAsync("user@test.com", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ShouldNotThrowAndNotSendEmail()
    {
        // Không ném lỗi để tránh dò tìm email
        var dto = new ForgotPasswordDto { Email = "nobody@test.com" };
        var act = () => CreateService().ForgotPasswordAsync(dto);

        await act.Should().NotThrowAsync();
        _emailMock.Verify(e => e.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_ShouldHashNewPasswordAndClearToken()
    {
        // Setup: tạo token trước
        using (var ctx = _factory.CreateContext())
        {
            var user = await ctx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
            user.PasswordResetToken = "valid-reset-token";
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            await ctx.SaveChangesAsync();
        }

        var dto = new ResetPasswordDto { Token = "valid-reset-token", NewPassword = "NewPass@999" };
        await CreateService().ResetPasswordAsync(dto);

        using var verifyCtx = _factory.CreateContext();
        var updated = await verifyCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
        BCrypt.Net.BCrypt.Verify("NewPass@999", updated.PasswordHash).Should().BeTrue("new password should be hashed and verifiable");
        updated.PasswordResetToken.Should().BeNull("token must be cleared to prevent Replay Attack");
        updated.PasswordResetTokenExpiry.Should().BeNull();
    }

    [Fact]
    public async Task ResetPassword_WithExpiredToken_ShouldThrow()
    {
        using (var ctx = _factory.CreateContext())
        {
            var user = await ctx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == TestDbFactory.UserId);
            user.PasswordResetToken = "expired-token";
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddMinutes(-1); // already expired
            await ctx.SaveChangesAsync();
        }

        var dto = new ResetPasswordDto { Token = "expired-token", NewPassword = "NewPass@999" };
        var act = () => CreateService().ResetPasswordAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid or expired*");
    }

    [Fact]
    public async Task VerifyEmail_WithValidToken_ShouldMarkVerifiedAndClearToken()
    {
        // Setup user chưa verify với token hợp lệ
        var unverifiedId = Guid.NewGuid();
        using (var ctx = _factory.CreateContext())
        {
            ctx.Users.Add(new User
            {
                Id = unverifiedId,
                Email = "toverify@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
                IsEmailVerified = false,
                EmailVerificationToken = "my-verify-token",
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                Status = TechGearAuction.Domain.Enums.UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var result = await CreateService().VerifyEmailAsync("toverify@test.com", "my-verify-token");

        result.Should().BeTrue();

        using var verifyCtx = _factory.CreateContext();
        var verified = await verifyCtx.Users.IgnoreQueryFilters().FirstAsync(u => u.Id == unverifiedId);
        verified.IsEmailVerified.Should().BeTrue();
        verified.EmailVerificationToken.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmail_WithWrongToken_ShouldReturnFalse()
    {
        // Tạo user chưa verify với token đã biết
        using (var ctx = _factory.CreateContext())
        {
            ctx.Users.Add(new Domain.Entities.User
            {
                Id = Guid.NewGuid(),
                Email = "wrongtoken@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
                IsEmailVerified = false,
                EmailVerificationToken = "correct-token",
                EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
                Status = TechGearAuction.Domain.Enums.UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var result = await CreateService().VerifyEmailAsync("wrongtoken@test.com", "WRONG-token");
        result.Should().BeFalse("wrong token must be rejected");
    }

    public void Dispose() => _factory.Dispose();
}

