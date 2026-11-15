using System.Security.Claims;
using IndieVault.Middlewares;
using IndieVault.Tests.TestHelpers;
using Microsoft.AspNetCore.Http;

namespace IndieVault.Tests.Middlewares
{
    public class RequestLoggingMiddlewareTests
    {
        [Fact]
        public async Task InvokeAsync_ForPageRequest_LogsIncomingAndOutgoing()
        {
            var context = new DefaultHttpContext();
            context.Request.Method = "GET";
            context.Request.Path = "/Game/Details/1";
            var logger = new ListLogger<RequestLoggingMiddleware>();
            var middleware = new RequestLoggingMiddleware(
                ctx => { ctx.Response.StatusCode = StatusCodes.Status200OK; return Task.CompletedTask; },
                logger);

            await middleware.InvokeAsync(context);

            Assert.Equal(2, logger.Entries.Count);
            Assert.Contains("Incoming request", logger.Entries[0].Message);
            Assert.Contains("Outgoing response", logger.Entries[1].Message);
            Assert.Contains("200", logger.Entries[1].Message);
        }

        [Theory]
        [InlineData("/css/site.css")]
        [InlineData("/js/site.js")]
        [InlineData("/images/games/1/cover.png")]
        public async Task InvokeAsync_ForStaticFile_SkipsLoggingButCallsNext(string path)
        {
            var context = new DefaultHttpContext();
            context.Request.Path = path;
            var logger = new ListLogger<RequestLoggingMiddleware>();
            var nextCalled = false;
            var middleware = new RequestLoggingMiddleware(
                _ => { nextCalled = true; return Task.CompletedTask; },
                logger);

            await middleware.InvokeAsync(context);

            Assert.True(nextCalled);
            Assert.Empty(logger.Entries);
        }

        [Fact]
        public async Task InvokeAsync_AnonymousUser_LogsAsAnonymous()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/";
            var logger = new ListLogger<RequestLoggingMiddleware>();
            var middleware = new RequestLoggingMiddleware(_ => Task.CompletedTask, logger);

            await middleware.InvokeAsync(context);

            Assert.Contains("Anonymous", logger.Entries[0].Message);
        }

        [Fact]
        public async Task InvokeAsync_AuthenticatedUser_LogsUserName()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/";
            var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "umer") }, "TestAuth");
            context.User = new ClaimsPrincipal(identity);
            var logger = new ListLogger<RequestLoggingMiddleware>();
            var middleware = new RequestLoggingMiddleware(_ => Task.CompletedTask, logger);

            await middleware.InvokeAsync(context);

            Assert.Contains("umer", logger.Entries[0].Message);
        }
    }
}
