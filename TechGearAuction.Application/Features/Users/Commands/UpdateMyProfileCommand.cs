using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class UpdateMyProfileCommand : IRequest
{
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = new List<SocialLinkDto>();
}

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMyProfileCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _context.Users.Include(u => u.SocialLinks).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        user.DisplayName = request.DisplayName;
        user.PhoneNumber = request.PhoneNumber;

        if (user.SocialLinks.Any())
        {
            _context.UserSocialLinks.RemoveRange(user.SocialLinks);
        }

        if (request.SocialLinks != null && request.SocialLinks.Any())
        {
            var newLinks = request.SocialLinks.Select(link => new UserSocialLink
            {
                UserId = userId,
                Platform = link.Platform,
                Url = link.Url
            }).ToList();
            _context.UserSocialLinks.AddRange(newLinks);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

