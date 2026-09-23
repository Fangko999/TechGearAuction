using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.DTOs.Auth;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auth;

public class LoginTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IJwtProvider> _jwtMock;

    public LoginTests()
    {
        _factory = new TestDbFactory();
        _emailMock = new Mock<IEmailService>();
        _jwtMock = new Mock<IJwtProvider>();
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");
    }

    private AuthService CreateService() =>
        new AuthService(_factory.CreateContext(), _jwtMock.Object, _emailMock.Object);

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        var dto = new LoginDto { Email = "user@test.com", Password = "User@123" };

        var result = await CreateService().LoginAsync(dto, "1.2.3.4", "device-abc");

        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.Email.Should().Be("user@test.com");
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldUpdateLastLoginIpAndCreateDeviceLog()
    {
        var dto = new LoginDto { Email = "user@test.com", Password = "User@123" };

        await CreateService().LoginAsync(dto, "99.88.77.66", "device-xyz");

        using var ctx = _factory.CreateContext();
        var user = await ctx.Users.FirstAsync(u => u.Email == "user@test.com");
        user.LastLoginIp.Should().Be("99.88.77.66");
        user.LastLoginDeviceHash.Should().Be("device-xyz");

        var deviceLog = await ctx.UserDeviceLogs.FirstOrDefaultAsync(d => d.UserId == user.Id);
        deviceLog.Should().NotBeNull();
        deviceLog!.IpAddress.Should().Be("99.88.77.66");
        deviceLog.DeviceHash.Should().Be("device-xyz");
    }

    [Fact]
    public async Task Login_WithUnverifiedEmail_ShouldThrow()
    {
        // Tạo user chưa verify
        using (var ctx = _factory.CreateContext())
        {
            ctx.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Email = "unverified@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Pass@123"),
                IsEmailVerified = false,
                Status = TechGearAuction.Domain.Enums.UserStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
            await ctx.SaveChangesAsync();
        }

        var dto = new LoginDto { Email = "unverified@test.com", Password = "Pass@123" };
        var act = () => CreateService().LoginAsync(dto, "1.1.1.1", "dev");

        await act.Should().ThrowAsync<Exception>().WithMessage("*verify your email*");
    }

    [Fact]
    public async Task Login_WithBannedDevice_ShouldThrow()
    {
        // Thêm device bị ban
        using (var ctx = _factory.CreateContext())
        {
            ctx.BannedDevices.Add(new BannedDevice
            {
                DeviceHash = "banned-device-hash",
                Reason = "Spam"
            });
            await ctx.SaveChangesAsync();
        }

        var dto = new LoginDto { Email = "user@test.com", Password = "User@123" };
        var act = () => CreateService().LoginAsync(dto, "1.1.1.1", "banned-device-hash");

        await act.Should().ThrowAsync<Exception>().WithMessage("*banned*");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ShouldThrow()
    {
        var dto = new LoginDto { Email = "user@test.com", Password = "WrongPass!" };
        var act = () => CreateService().LoginAsync(dto, "1.1.1.1", "dev");

        await act.Should().ThrowAsync<Exception>().WithMessage("*Invalid*");
    }

    public void Dispose() => _factory.Dispose();
}

