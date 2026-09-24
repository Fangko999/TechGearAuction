using FluentAssertions;
using Moq;
using TechGearAuction.Application.Features.Appeals.Commands;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;
using Microsoft.EntityFrameworkCore;

namespace TechGearAuction.Tests.Appeals;

public class SubmitBanAppealTests : IDisposable
{
    private readonly TestDbFactory _factory;
    private readonly Mock<IStorageService> _storageMock;

    public SubmitBanAppealTests()
    {
        _factory = new TestDbFactory();
        _storageMock = new Mock<IStorageService>();
    }

    [Fact]
    public async Task SubmitBanAppeal_ForBannedUser_ShouldCreateAppealAndUploadFiles()
    {
        using (var setupCtx = _factory.CreateContext())
        {
            var user = await setupCtx.Users.FindAsync(TestDbFactory.UserId);
            user!.Status = UserStatus.Banned;
            await setupCtx.SaveChangesAsync();
        }

        _storageMock.Setup(s => s.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                    .ReturnsAsync("http://minio/bucket/file.jpg");

        var handler = new SubmitBanAppealCommandHandler(_factory.CreateContext(), _storageMock.Object);

        var evidences = new List<AppealEvidenceDto>
        {
            new AppealEvidenceDto { Stream = new MemoryStream(), FileName = "test.jpg", ContentType = "image/jpeg" }
        };

        var appealId = await handler.Handle(new SubmitBanAppealCommand
        {
            Email = "user@test.com",
            Description = "I was hacked",
            Evidences = evidences
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var appeal = await verifyCtx.Appeals.Include(a => a.Evidences).FirstOrDefaultAsync(a => a.Id == appealId);

        appeal.Should().NotBeNull();
        appeal!.UserId.Should().Be(TestDbFactory.UserId);
        appeal.Status.Should().Be(AppealStatus.Pending);
        appeal.Evidences.Should().HaveCount(1);
        appeal.Evidences.First().MediaUrl.Should().Be("http://minio/bucket/file.jpg");
    }

    [Fact]
    public async Task SubmitBanAppeal_ForActiveUser_ShouldThrow()
    {
        var handler = new SubmitBanAppealCommandHandler(_factory.CreateContext(), _storageMock.Object);

        var act = () => handler.Handle(new SubmitBanAppealCommand
        {
            Email = "user@test.com",
            Description = "Appeal"
        }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}

