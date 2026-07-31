using System.Net;
using System.Text.Json;
using SmartExpenseTracker.Common.Exceptions;

namespace SmartExpenseTracker.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred during request processing.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        object responsePayload;

        switch (exception)
        {
            case ValidationException valEx:
                statusCode = HttpStatusCode.BadRequest;
                responsePayload = new
                {
                    status = (int)statusCode,
                    error = "Bad Request",
                    message = valEx.Message,
                    details = valEx.Errors
                };
                break;

            case NotFoundException notFoundEx:
                statusCode = HttpStatusCode.NotFound;
                responsePayload = new
                {
                    status = (int)statusCode,
                    error = "Not Found",
                    message = notFoundEx.Message
                };
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                responsePayload = new
                {
                    status = (int)statusCode,
                    error = "Internal Server Error",
                    message = "An unexpected error occurred while processing your request."
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsync(JsonSerializer.Serialize(responsePayload, jsonOptions));
    }
}
