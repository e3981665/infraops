namespace InfraOps.Api.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private const string ContentSecurityPolicy =
        "default-src 'none'; frame-ancestors 'none'; base-uri 'none'";

    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        headers.TryAdd("X-Content-Type-Options", "nosniff");
        headers.TryAdd("X-Frame-Options", "DENY");
        headers.TryAdd("Referrer-Policy", "no-referrer");
        headers.TryAdd("Permissions-Policy", "camera=(), microphone=(), geolocation=()");
        headers.TryAdd("Content-Security-Policy", ContentSecurityPolicy);

        return _next(context);
    }
}
