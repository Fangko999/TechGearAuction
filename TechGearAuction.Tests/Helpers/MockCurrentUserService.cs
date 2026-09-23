using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Tests.Helpers;

/// <summary>
/// Mock ICurrentUserService — cho phép test giả lập user bất kỳ đang đăng nhập.
/// </summary>
public class MockCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = "User";

    public MockCurrentUserService(Guid userId, string role = "User")
    {
        UserId = userId;
        Role = role;
    }
}
