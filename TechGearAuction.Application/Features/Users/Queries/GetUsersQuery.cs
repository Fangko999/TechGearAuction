using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.DTOs.User;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Queries;

public class GetUsersQuery : IRequest<PagedResult<UserProfileDto>>
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserProfileDto>>
{
    private readonly IAppDbContext _context;

    public GetUsersQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<UserProfileDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Users.AsNoTracking()
            .IgnoreQueryFilters()
            .Include(u => u.SocialLinks)
            .OrderByDescending(u => u.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var users = await query
            .Skip((request.PageIndex - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = users.Select(user => new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            AvailableCredits = user.AvailableCredits,
            Role = user.Role.ToString(),
            Status = user.Status.ToString(),
            IsEmailVerified = user.IsEmailVerified,
            CreatedAt = user.CreatedAt,
            ViolationCount = user.ViolationCount,
            ReportedAsSellerCount = user.ReportedAsSellerCount,
            ReportedAsBuyerCount = user.ReportedAsBuyerCount,
            SuspiciousBidderCount = user.SuspiciousBidderCount,
            SuspiciousSellerCount = user.SuspiciousSellerCount,
            SocialLinks = user.SocialLinks.Select(link => new SocialLinkDto
            {
                Platform = link.Platform ?? "Unknown",
                Url = link.Url
            }).ToList()
        }).ToList();

        return new PagedResult<UserProfileDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageIndex = request.PageIndex,
            PageSize = request.PageSize
        };
    }
}
