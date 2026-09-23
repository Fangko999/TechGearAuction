using MediatR;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Application.Features.Chat.Commands;

public class UploadChatMediaCommand : IRequest<string>
{
    public Stream FileStream { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string ContentType { get; set; } = null!;
}

public class UploadChatMediaCommandHandler : IRequestHandler<UploadChatMediaCommand, string>
{
    private readonly IStorageService _storageService;

    public UploadChatMediaCommandHandler(IStorageService storageService)
    {
        _storageService = storageService;
    }

    public async Task<string> Handle(UploadChatMediaCommand request, CancellationToken cancellationToken)
    {
        if (request.FileStream == null || request.FileStream.Length == 0)
            throw new Exception("File is empty.");

        var fileName = $"{Guid.NewGuid()}_{request.FileName}";
        var url = await _storageService.UploadFileAsync(request.FileStream, fileName, request.ContentType, "chat-media");
        
        return url;
    }
}
