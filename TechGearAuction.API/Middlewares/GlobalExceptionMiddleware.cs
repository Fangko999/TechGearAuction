using System.Net;
using System.Text.Json;
using TechGearAuction.Domain.Exceptions;

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
            case NotFoundException or KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                break;
            case ValidationException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case ForbiddenException or BannedUserException:
                statusCode = HttpStatusCode.Forbidden;
                message = exception.Message;
                break;
            case BusinessRuleException or ArgumentException or ArgumentNullException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = exception.Message;
                break;
            case ConcurrencyException:
                statusCode = HttpStatusCode.Conflict;
                message = exception.Message;
                break;
            default:
                message = "An unexpected error occurred. Please try again later.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var result = JsonSerializer.Serialize(new { Message = message, StatusCode = (int)statusCode });
        return context.Response.WriteAsync(result);
    }
}

