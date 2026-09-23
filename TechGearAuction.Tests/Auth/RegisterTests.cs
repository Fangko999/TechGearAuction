using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.DTOs.Auth;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Infrastructure.Services;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Auth;

public class RegisterTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IEmailService> _emailMock;
    private readonly Mock<IJwtProvider> _jwtMock;

    public RegisterTests()
    {
        _factory = new TestDbFactory();
        _emailMock = new Mock<IEmailService>();
        _jwtMock = new Mock<IJwtProvider>();
        _jwtMock.Setup(j => j.GenerateToken(It.IsAny<User>())).Returns("fake-jwt-token");
    }

    private AuthService CreateService() =>
        new AuthService(_factory.CreateContext(), _jwtMock.Object, _emailMock.Object);

    [Fact]
    public async Task Register_WithValidData_ShouldCreateUserAndSendEmail()
    {
        var dto = new RegisterDto
        {
            Email = "newuser@test.com",
            Password = "NewPass@123",
            DisplayName = "New User"
        };

        var svc = CreateService();
        await svc.RegisterAsync(dto);

        using var ctx = _factory.CreateContext();
        var user = await ctx.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == dto.Email);
        user.Should().NotBeNull();
        user!.IsEmailVerified.Should().BeFalse();
        user.EmailVerificationToken.Should().NotBeNullOrEmpty();
        user.AvailableCredits.Should().Be(3);

        // Signup bonus credit transaction phải được tạo
        var txExists = await ctx.CreditTransactions.AnyAsync(t => t.UserId == user.Id && t.Reason == "Signup Bonus");
        txExists.Should().BeTrue();

        _emailMock.Verify(e => e.SendEmailAsync(dto.Email, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldThrow()
    {
        var dto = new RegisterDto
        {
            Email = "user@test.com", // already exists in seed
            Password = "Pass@123",
            DisplayName = "Duplicate"
        };

        var svc = CreateService();
        var act = () => svc.RegisterAsync(dto);

        await act.Should().ThrowAsync<Exception>().WithMessage("*already exists*");
    }

    public void Dispose() => _factory.Dispose();
}

