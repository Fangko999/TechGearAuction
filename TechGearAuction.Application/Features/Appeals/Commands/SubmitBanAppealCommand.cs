using MediatR;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Application.Features.Appeals.Commands;

public class AppealEvidenceDto
{
    public Stream Stream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class SubmitBanAppealCommand : IRequest<Guid>
{
    public string Email { get; set; } = null!;
    public string Description { get; set; } = null!;
    public List<AppealEvidenceDto> Evidences { get; set; } = new List<AppealEvidenceDto>();
}

public class SubmitBanAppealCommandHandler : IRequestHandler<SubmitBanAppealCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly IStorageService _storageService;

    public SubmitBanAppealCommandHandler(IAppDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<Guid> Handle(SubmitBanAppealCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);
        if (user == null)
            throw new Exception("User not found.");

        if (user.Status != UserStatus.Banned)
            throw new Exception("This account is not banned. No appeal needed.");

        var appeal = new Appeal
        {
            UserId = user.Id,
            Description = request.Description,
            Status = AppealStatus.Pending
        };

        foreach (var file in request.Evidences)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var contentType = file.ContentType;

            var bucketName = contentType.StartsWith("image/") ? "images" : "chat-media"; // minio bucket
            
            var url = await _storageService.UploadFileAsync(file.Stream, fileName, contentType, bucketName);
            
            appeal.Evidences.Add(new AppealEvidence
            {
                AppealId = appeal.Id,
                MediaUrl = url,
                Type = contentType.StartsWith("image/") ? MediaType.Image : MediaType.Video
            });
        }

        _context.Appeals.Add(appeal);
        await _context.SaveChangesAsync(cancellationToken);

        return appeal.Id;
    }
}
