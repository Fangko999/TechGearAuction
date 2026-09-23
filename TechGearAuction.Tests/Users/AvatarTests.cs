using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechGearAuction.Application.Features.Users.Commands;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Users;

public class AvatarTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IStorageService> _storageMock;

    public AvatarTests()
    {
        _factory = new TestDbFactory();
        _storageMock = new Mock<IStorageService>();
    }

    [Fact]
    public async Task UpdateAvatar_ShouldUploadAndPersistUrl()
    {
        // Setup mock to return a fake URL
        _storageMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("https://fake-minio.local/avatars/new-avatar.jpg");

        var svc = new MockCurrentUserService(TestDbFactory.UserId);
        var ctx = _factory.CreateContext();
        var minioOptionsMock = new Mock<Microsoft.Extensions.Options.IOptions<TechGearAuction.Application.Common.Models.MinioSettings>>();
        minioOptionsMock.Setup(o => o.Value).Returns(new TechGearAuction.Application.Common.Models.MinioSettings 
        { 
            Buckets = new TechGearAuction.Application.Common.Models.MinioBuckets { Avatars = "avatars" } 
        });

        var handler = new UpdateAvatarCommandHandler(ctx, svc, _storageMock.Object, minioOptionsMock.Object);

        using var fakeStream = new MemoryStream();
        var command = new UpdateAvatarCommand
        {
            FileStream = fakeStream,
            FileName = "test.jpg",
            ContentType = "image/jpeg"
        };

        var result = await handler.Handle(command, CancellationToken.None);

        result.Should().Be("https://fake-minio.local/avatars/new-avatar.jpg");

        using var verifyCtx = _factory.CreateContext();
        var user = await verifyCtx.Users.FirstAsync(u => u.Id == TestDbFactory.UserId);
        user.AvatarUrl.Should().Be("https://fake-minio.local/avatars/new-avatar.jpg");
    }

    public void Dispose() => _factory.Dispose();
}
