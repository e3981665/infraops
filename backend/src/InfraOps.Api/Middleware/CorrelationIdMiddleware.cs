using InfraOps.Api.Logging;
using Serilog.Context;

namespace InfraOps.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-ID";
    private const string ContextItemName = "CorrelationId";
    private const int MaxCorrelationIdLength = 128;

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.TraceIdentifier = correlationId;
        context.Items[ContextItemName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>()
                   .BeginScope(new Dictionary<string, object>
                   {
                       ["CorrelationId"] = correlationId
                   }))
        {
            await _next(context);
        }
    }

    public static string GetCorrelationId(HttpContext context)
    {
        return context.Items.TryGetValue(ContextItemName, out var value) && value is string correlationId
            ? correlationId
            : context.TraceIdentifier;
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        var incomingCorrelationId = context.Request.Headers[HeaderName].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(incomingCorrelationId))
        {
            return LogSanitizer.Sanitize(incomingCorrelationId.Trim(), MaxCorrelationIdLength);
        }

        return LogSanitizer.Sanitize(context.TraceIdentifier, MaxCorrelationIdLength);
    }
}
