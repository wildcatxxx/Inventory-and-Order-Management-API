using System.Diagnostics;
using Serilog.Context;

namespace InventoryAPI.Infrastructure;

/// <summary>
/// Middleware for logging HTTP requests and responses with structured logging using Serilog.
/// Captures method, path, query, status code, response time, and user context.
/// </summary>
public class SerilogHttpLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SerilogHttpLoggingMiddleware> _logger;

    public SerilogHttpLoggingMiddleware(RequestDelegate next, ILogger<SerilogHttpLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Store original body streams for potential re-reading
        var originalBodyStream = context.Response.Body;
        
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            var stopwatch = Stopwatch.StartNew();
            var requestPath = context.Request.Path;
            var requestMethod = context.Request.Method;
            var queryString = context.Request.QueryString.Value;

            try
            {
                // Extract and enrich with user context if authenticated
                if (context.User?.FindFirst("sub")?.Value is { Length: > 0 } userId)
                {
                    LogContext.PushProperty("UserId", userId);
                }
                
                if (context.User?.FindFirst("email")?.Value is { Length: > 0 } email)
                {
                    LogContext.PushProperty("UserEmail", email);
                }

                // Log incoming request
                _logger.LogInformation(
                    "HTTP request started: {Method} {Path}{QueryString} from {RemoteIP}",
                    requestMethod,
                    requestPath,
                    queryString,
                    context.Connection.RemoteIpAddress);

                await _next(context);

                stopwatch.Stop();

                // Copy response body back to original stream
                responseBody.Position = 0;
                await responseBody.CopyToAsync(originalBodyStream);

                // Log completed request with response details
                _logger.LogInformation(
                    "HTTP request completed: {Method} {Path} responded with {StatusCode} in {ElapsedMilliseconds}ms",
                    requestMethod,
                    requestPath,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(
                    ex,
                    "HTTP request failed: {Method} {Path} with exception after {ElapsedMilliseconds}ms",
                    requestMethod,
                    requestPath,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}

/// <summary>
/// Extension method to add SerilogHttpLoggingMiddleware to the application pipeline.
/// </summary>
public static class SerilogHttpLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseSerilogHttpLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SerilogHttpLoggingMiddleware>();
    }
}
