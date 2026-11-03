using IndieVault.Middlewares;

namespace IndieVault.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();

    public static IApplicationBuilder UseRequestLoggingMiddleware(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}
