using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TechGearAuction.Application.Features.Reports.Commands;
using TechGearAuction.Application.Features.Reports.Queries;
using TechGearAuction.Domain.Entities;
using TechGearAuction.Domain.Enums;
using TechGearAuction.Tests.Helpers;

namespace TechGearAuction.Tests.Admin;

public class AdminReportTests : IDisposable
{
    private readonly TestDbFactory _factory;

    public AdminReportTests()
    {
        _factory = new TestDbFactory();
    }

    [Fact]
    public async Task ResolveReport_IssueStrike_ShouldIncreaseViolationCount()
    {
        var reportId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.Reports.Add(new Report
            {
                Id = reportId,
                ReporterId = TestDbFactory.AdminId,
                ReportedUserId = TestDbFactory.UserId,
                Type = ReportType.Scam,
                Status = ReportStatus.Pending
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var handler = new ResolveReportCommandHandler(_factory.CreateContext(), svc);

        await handler.Handle(new ResolveReportCommand
        {
            ReportId = reportId,
            Action = ResolutionAction.IssueStrike,
            AdminNote = "Strike 1"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var user = await verifyCtx.Users.FindAsync(TestDbFactory.UserId);
        var report = await verifyCtx.Reports.FindAsync(reportId);

        user!.ViolationCount.Should().Be(1);
        user.Status.Should().Be(UserStatus.Active);
        report!.Status.Should().Be(ReportStatus.Resolved);
        
        var audit = await verifyCtx.AdminAuditLogs.FirstOrDefaultAsync(l => l.AdminId == TestDbFactory.AdminId);
        audit.Should().NotBeNull();
    }

    [Fact]
    public async Task ResolveReport_IssueStrike5Times_ShouldAutoBan()
    {
        var reportId = Guid.NewGuid();
        using (var setupCtx = _factory.CreateContext())
        {
            var user = await setupCtx.Users.FindAsync(TestDbFactory.UserId);
            user!.ViolationCount = 4;
            user!.LastLoginDeviceHash = "Device123";

            setupCtx.Reports.Add(new Report
            {
                Id = reportId,
                ReporterId = TestDbFactory.AdminId,
                ReportedUserId = TestDbFactory.UserId,
                Type = ReportType.Scam,
                Status = ReportStatus.Pending
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var handler = new ResolveReportCommandHandler(_factory.CreateContext(), svc);

        await handler.Handle(new ResolveReportCommand
        {
            ReportId = reportId,
            Action = ResolutionAction.IssueStrike,
            AdminNote = "Strike 5"
        }, CancellationToken.None);

        using var verifyCtx = _factory.CreateContext();
        var verifyUser = await verifyCtx.Users.FindAsync(TestDbFactory.UserId);
        
        verifyUser!.ViolationCount.Should().Be(5);
        verifyUser.Status.Should().Be(UserStatus.Banned);

        var banLog = await verifyCtx.BannedDevices.FirstOrDefaultAsync(b => b.DeviceHash == "Device123");
        banLog.Should().NotBeNull();
    }

    [Fact]
    public async Task GetChatHistory_ShouldReturnMessagesAndLogAudit()
    {
        var reportId = Guid.NewGuid();
        var chatRoomId = Guid.NewGuid();

        using (var setupCtx = _factory.CreateContext())
        {
            setupCtx.ChatRooms.Add(new ChatRoom
            {
                Id = chatRoomId,
                AuctionId = TestDbFactory.ActiveAuctionId,
                Status = ChatRoomStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            setupCtx.ChatMessages.Add(new ChatMessage
            {
                ChatRoomId = chatRoomId,
                SenderId = TestDbFactory.UserId,
                Content = "Test",
                MessageType = ChatMessageType.Text
            });

            setupCtx.Reports.Add(new Report
            {
                Id = reportId,
                ReporterId = TestDbFactory.AdminId,
                ReportedUserId = TestDbFactory.UserId,
                ChatRoomId = chatRoomId,
                Type = ReportType.Scam,
                Status = ReportStatus.Pending
            });
            await setupCtx.SaveChangesAsync();
        }

        var svc = new MockCurrentUserService(TestDbFactory.AdminId);
        var handler = new GetAdminReportChatHistoryQueryHandler(_factory.CreateContext(), svc);

        var result = await handler.Handle(new GetAdminReportChatHistoryQuery { ReportId = reportId }, CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Content.Should().Be("Test");

        using var verifyCtx = _factory.CreateContext();
        var audit = await verifyCtx.AdminAuditLogs.FirstOrDefaultAsync(l => l.Action == "ViewReportChatHistory");
        audit.Should().NotBeNull();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
