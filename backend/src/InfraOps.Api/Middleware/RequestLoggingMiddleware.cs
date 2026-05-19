using System.Diagnostics;

namespace InfraOps.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);

        _logger.LogInformation(
            "HTTP request started {Method} {Path} with correlation {CorrelationId}.",
            context.Request.Method,
            context.Request.Path.Value,
            correlationId);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "HTTP request completed {Method} {Path} with {StatusCode} in {ElapsedMilliseconds} ms and correlation {CorrelationId}.",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationId);
        }
    }
}
