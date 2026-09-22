using Microsoft.Extensions.Logging;
using TechGearAuction.Application.Interfaces;

namespace TechGearAuction.Infrastructure.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation("========== MOCK EMAIL ==========");
        _logger.LogInformation($"To: {to}");
        _logger.LogInformation($"Subject: {subject}");
        _logger.LogInformation($"Body: {body}");
        _logger.LogInformation("================================");
        
        return Task.CompletedTask;
    }
}

