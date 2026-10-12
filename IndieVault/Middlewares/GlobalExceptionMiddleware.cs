namespace IndieVault.Middlewares
{
	public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
	{
	    public async Task InvokeAsync(HttpContext context)
	    {
		try
		{
		    await next(context);
		}
		catch (Exception ex)
		{
		    logger.LogCritical(ex, "Unhandled exception for {Method} {Path}",
		        context.Request.Method, context.Request.Path);

		    // Once the response has started, headers can't be changed.
		    if (context.Response.HasStarted)
		    {
		        return;
		    }

		    context.Response.Redirect("/Error");
		}
	    }
	}
}
