using TechGearAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechGearAuction.Application.Common.Models;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Users.Commands;

public class UpdateAvatarCommand : IRequest<string>
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class UpdateAvatarCommandHandler : IRequestHandler<UpdateAvatarCommand, string>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IStorageService _storageService;
    private readonly MinioSettings _minioSettings;

    public UpdateAvatarCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, IStorageService storageService, IOptions<MinioSettings> minioSettings)
    {
        _context = context;
        _currentUserService = currentUserService;
        _storageService = storageService;
        _minioSettings = minioSettings.Value;
    }

    public async Task<string> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Entity", "User not found.");
        }

        var extension = Path.GetExtension(request.FileName);
        var newFileName = $"{userId}_{Guid.NewGuid()}{extension}";
        
        var avatarUrl = await _storageService.UploadFileAsync(request.FileStream, newFileName, request.ContentType, _minioSettings.Buckets.Avatars);
        
        user.AvatarUrl = avatarUrl;
        await _context.SaveChangesAsync(cancellationToken);
        
        return avatarUrl;
    }
}

