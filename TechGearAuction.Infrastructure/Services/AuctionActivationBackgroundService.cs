using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Domain.Enums;

namespace TechGearAuction.Infrastructure.Services;

public class AuctionActivationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuctionActivationBackgroundService> _logger;

    public AuctionActivationBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AuctionActivationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessScheduledAuctionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing scheduled auctions.");
            }

            // Polling interval
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcessScheduledAuctionsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var auctionsToActivate = await context.Auctions
            .Where(a => a.Status == AuctionStatus.Scheduled && a.StartTime <= DateTime.UtcNow)
            .ToListAsync(stoppingToken);

        if (auctionsToActivate.Any())
        {
            foreach (var auction in auctionsToActivate)
            {
                auction.Status = AuctionStatus.Active;
                _logger.LogInformation("Auction {AuctionId} has been activated.", auction.Id);
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
