using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;

namespace TechGearAuction.Application.Features.Users.Commands;

public class UpdateUserByAdminCommand : IRequest
{
    public Guid TargetUserId { get; set; }
    public string? DisplayName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public List<SocialLinkDto> SocialLinks { get; set; } = new List<SocialLinkDto>();
}

public class UpdateUserByAdminCommandHandler : IRequestHandler<UpdateUserByAdminCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateUserByAdminCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateUserByAdminCommand request, CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;
        var targetUser = await _context.Users.Include(u => u.SocialLinks).FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken);
        
        if (targetUser == null)
        {
            throw new NotFoundException("Entity", "User not found.");
        }

        targetUser.DisplayName = request.DisplayName;
        targetUser.PhoneNumber = request.PhoneNumber;
        targetUser.AvatarUrl = request.AvatarUrl;

        if (targetUser.SocialLinks.Any())
        {
            _context.UserSocialLinks.RemoveRange(targetUser.SocialLinks);
        }

        if (request.SocialLinks != null && request.SocialLinks.Any())
        {
            var newLinks = request.SocialLinks.Select(link => new UserSocialLink
            {
                UserId = request.TargetUserId,
                Platform = link.Platform,
                Url = link.Url
            }).ToList();
            _context.UserSocialLinks.AddRange(newLinks);
        }

        var auditLog = new AdminAuditLog
        {
            AdminId = adminId,
            Action = "Update Profile",
            EntityType = "User",
            EntityId = request.TargetUserId,
            Details = $"Admin updated profile of user {request.TargetUserId}"
        };
        _context.AdminAuditLogs.Add(auditLog);

        await _context.SaveChangesAsync(cancellationToken);
    }
}


