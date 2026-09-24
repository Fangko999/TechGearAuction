using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TechGearAuction.Application.Interfaces;
using TechGearAuction.Infrastructure.Data;

namespace TechGearAuction.Tests.Helpers;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IAppDbContext));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            var dbName = "InMemoryDbForTesting_" + Guid.NewGuid().ToString();
            
            services.AddScoped<IAppDbContext>(sp => 
            {
                var options = new DbContextOptionsBuilder<AppDbContext>()
                    .UseInMemoryDatabase(dbName)
                    .Options;
                var context = new AppDbContext(options);
                context.Database.EnsureCreated();
                return context;
            });
        });
    }
}
