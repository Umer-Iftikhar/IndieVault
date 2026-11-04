using IndieVault.Middlewares;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IndieVault.Tests.Middlewares
{
    public class GlobalExceptionMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_WhenNextThrows_RedirectsToErrorPage()
        {
            var context = new DefaultHttpContext();
            var middleware = new GlobalExceptionMiddleware(
                _ => throw new InvalidOperationException("boom"),
                NullLogger<GlobalExceptionMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.Equal(StatusCodes.Status302Found, context.Response.StatusCode);
            Assert.Equal("/Error", context.Response.Headers.Location.ToString());
        }

        [Fact]
        public async Task InvokeAsync_WhenNextThrows_LogsCriticalWithException()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/Game/Details/5";
            var logger = new ListLogger<GlobalExceptionMiddleware>();
            var exception = new InvalidOperationException("boom");
            var middleware = new GlobalExceptionMiddleware(_ => throw exception, logger);

            await middleware.InvokeAsync(context);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Critical, entry.Level);
            Assert.Same(exception, entry.Exception);
            Assert.Contains("/Game/Details/5", entry.Message);
        }

        [Fact]
        public async Task InvokeAsync_WhenNextSucceeds_LeavesResponseUntouched()
        {
            var context = new DefaultHttpContext();
            var middleware = new GlobalExceptionMiddleware(
                ctx => { ctx.Response.StatusCode = StatusCodes.Status200OK; return Task.CompletedTask; },
                NullLogger<GlobalExceptionMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
            Assert.False(context.Response.Headers.ContainsKey("Location"));
        }

        [Fact]
        public async Task InvokeAsync_WhenResponseAlreadyStarted_DoesNotRedirect()
        {
            var context = new DefaultHttpContext();
            context.Features.Set<IHttpResponseFeature>(new StartedResponseFeature());
            var middleware = new GlobalExceptionMiddleware(
                _ => throw new InvalidOperationException("boom"),
                NullLogger<GlobalExceptionMiddleware>.Instance);

            await middleware.InvokeAsync(context);

            Assert.False(context.Response.Headers.ContainsKey("Location"));
        }

        private class StartedResponseFeature : HttpResponseFeature
        {
            public override bool HasStarted => true;
        }
    }
}
