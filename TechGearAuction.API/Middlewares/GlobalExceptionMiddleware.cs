using System.Net;
using System.Text.Json;

namespace TechGearAuction.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        switch (exception)
        {
            case ArgumentException or ArgumentNullException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;
            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            // Add other specific domain exceptions here (e.g., BannedUserException)
            default:
                // For development, we might want to return the actual exception message, but for prod it's better to hide it.
                // Assuming dev/debug environment based on previous logs, we'll return the message.
                message = exception.Message; 
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { Message = message, StatusCode = (int)statusCode });
        return context.Response.WriteAsync(result);
    }
}

