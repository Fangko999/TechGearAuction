namespace TechGearAuction.Application.Interfaces;

public interface ICurrentUserService
{
    int UserId { get; }
    string Role { get; }
}
