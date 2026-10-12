namespace IndieVault.Middlewares
{

	public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
	{
	    public async Task InvokeAsync(HttpContext context)
	    {
		var path = context.Request.Path.Value ?? string.Empty;

		// Skip static files (css, js, images)
		if (path.Contains('.'))
		{
		    await next(context);
		    return;
		}

		var userName = context.User.Identity?.IsAuthenticated == true
		    ? context.User.Identity.Name ?? "Anonymous"
		    : "Anonymous";

		logger.LogInformation("Incoming request {Method} {Path} by {User}",
		    context.Request.Method, path, userName);

		await next(context);

		logger.LogInformation("Outgoing response {Method} {Path} returned {StatusCode}",
		    context.Request.Method, path, context.Response.StatusCode);
	    }
	}
}
