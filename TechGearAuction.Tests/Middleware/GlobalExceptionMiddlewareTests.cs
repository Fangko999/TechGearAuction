using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using TechGearAuction.API.Middlewares;
using TechGearAuction.Domain.Exceptions;

namespace TechGearAuction.Tests.Middleware;

public class GlobalExceptionMiddlewareTests
{
    private TestServer CreateServer(Exception exceptionToThrow)
    {
        var builder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddLogging(builder => builder.ClearProviders()); // Suppress log output for tests
            })
            .Configure(app =>
            {
                app.UseMiddleware<GlobalExceptionMiddleware>();
                app.Run(context => throw exceptionToThrow);
            });

        return new TestServer(builder);
    }

    [Fact]
    public async Task WhenNotFoundException_ShouldReturn404WithJsonBody()
    {
        var server = CreateServer(new NotFoundException("Entity", "Not Found"));
        var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Not Found");
    }

    [Fact]
    public async Task WhenArgumentException_ShouldReturn400()
    {
        var server = CreateServer(new ArgumentException("Invalid arg"));
        var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Invalid arg");
    }
    
    [Fact]
    public async Task WhenBusinessRuleException_ShouldReturn400()
    {
        var server = CreateServer(new BusinessRuleException("Business rule violated"));
        var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenConcurrencyException_ShouldReturn409()
    {
        var server = CreateServer(new ConcurrencyException("Concurrency"));
        var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Concurrency");
    }

    [Fact]
    public async Task WhenUnhandledException_ShouldReturn500()
    {
        var server = CreateServer(new Exception("Unknown"));
        var client = server.CreateClient();

        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Unknown");
    }
}
