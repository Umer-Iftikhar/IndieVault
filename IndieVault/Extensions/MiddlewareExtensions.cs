using IndieVault.Web.Middleware;

namespace IndieVault.Web.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();

    public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
