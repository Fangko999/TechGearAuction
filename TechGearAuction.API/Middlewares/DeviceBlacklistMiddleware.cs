using TechGearAuction.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace TechGearAuction.API.Middlewares;

public class DeviceBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public DeviceBlacklistMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAppDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLower();

        // 1. Bypass Whitelist (Đục lỗ Middleware)
        if (path != null && path.Contains("/api/appeals/banned-users"))
        {
            await _next(context);
            return;
        }

        // 2. Extract Device Hash from header
        var deviceHash = context.Request.Headers["X-Device-Hash"].FirstOrDefault();
        if (!string.IsNullOrEmpty(deviceHash))
        {
            var isBanned = await dbContext.BannedDevices.AnyAsync(b => b.DeviceHash == deviceHash);
            if (isBanned)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                var response = new 
                { 
                    Message = "Your device has been banned from accessing the system. Please submit an appeal.",
                    CanAppeal = true
                };
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(context);
    }
}

