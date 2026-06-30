using System.Diagnostics;

namespace JobsMarketplace.Api.Middleware;

public class RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
{
    private static readonly TimeSpan SlowRequestThreshold = TimeSpan.FromMilliseconds(500);

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        await next(context);

        stopwatch.Stop();

        if (stopwatch.Elapsed >= SlowRequestThreshold)
        {
            logger.LogWarning(
                "Slow request detected: {Method} {Path} completed in {ElapsedMilliseconds} ms with status code {StatusCode}.",
                context.Request.Method,
                context.Request.Path,
                stopwatch.Elapsed.TotalMilliseconds,
                context.Response.StatusCode);
        }
        else
        {
            logger.LogInformation(
                "Request completed: {Method} {Path} in {ElapsedMilliseconds} ms with status code {StatusCode}.",
                context.Request.Method,
                context.Request.Path,
                stopwatch.Elapsed.TotalMilliseconds,
                context.Response.StatusCode);
        }
    }
}

