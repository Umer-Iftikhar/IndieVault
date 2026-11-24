using System.Net;
using System.Security.Claims;
using IndieVault.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IndieVault.Tests.Extensions
{
    // Runs the real rate limiter middleware in an in-memory test server (no database, no AI calls).
    public class AiRateLimitingTests
    {
        private const string UserHeader = "X-Test-User";

        private static Task<IHost> StartHostAsync()
        {
            RequestDelegate endpoint = context => context.Response.WriteAsync("ok");

            return new HostBuilder()
                .ConfigureWebHost(webHost => webHost
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddRouting();
                        services.AddAiRateLimiting();
                    })
                    .Configure(app =>
                    {
                        // Stand-in for authentication: the header value becomes the signed-in user id
                        app.Use(async (context, next) =>
                        {
                            if (context.Request.Headers.TryGetValue(UserHeader, out var userId))
                            {
                                var identity = new ClaimsIdentity(
                                    new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }, "TestAuth");
                                context.User = new ClaimsPrincipal(identity);
                            }

                            await next(context);
                        });

                        app.UseRouting();
                        app.UseRateLimiter();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapGet("/ai", endpoint).RequireRateLimiting(RateLimitPolicies.AiRecommendations);
                            endpoints.MapGet("/open", endpoint);
                        });
                    }))
                .StartAsync();
        }

        private static Task<HttpResponseMessage> GetAsync(HttpClient client, string path, string? userId = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, path);
            if (userId != null)
            {
                request.Headers.Add(UserHeader, userId);
            }

            return client.SendAsync(request);
        }

        [Fact]
        public async Task LimitedEndpoint_AllowsRequestsUpToTheLimit()
        {
            using var host = await StartHostAsync();
            var client = host.GetTestClient();

            for (var i = 0; i < RateLimitingExtensions.AiPermitLimit; i++)
            {
                var response = await GetAsync(client, "/ai", "player-1");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public async Task LimitedEndpoint_RequestOverTheLimit_Returns429()
        {
            using var host = await StartHostAsync();
            var client = host.GetTestClient();
            for (var i = 0; i < RateLimitingExtensions.AiPermitLimit; i++)
            {
                await GetAsync(client, "/ai", "player-1");
            }

            var response = await GetAsync(client, "/ai", "player-1");

            Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);
        }

        [Fact]
        public async Task Limit_IsCountedPerUser()
        {
            using var host = await StartHostAsync();
            var client = host.GetTestClient();
            for (var i = 0; i <= RateLimitingExtensions.AiPermitLimit; i++)
            {
                await GetAsync(client, "/ai", "player-1");
            }

            var otherPlayer = await GetAsync(client, "/ai", "player-2");

            Assert.Equal(HttpStatusCode.OK, otherPlayer.StatusCode);
        }

        [Fact]
        public async Task EndpointWithoutThePolicy_IsNeverLimited()
        {
            using var host = await StartHostAsync();
            var client = host.GetTestClient();

            for (var i = 0; i < RateLimitingExtensions.AiPermitLimit + 5; i++)
            {
                var response = await GetAsync(client, "/open", "player-1");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Fact]
        public void GetPartitionKey_SignedInUser_UsesUserId()
        {
            var context = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, "player-1") }, "TestAuth"))
            };

            Assert.Equal("player-1", RateLimitingExtensions.GetPartitionKey(context));
        }

        [Fact]
        public void GetPartitionKey_AnonymousWithIp_UsesIpAddress()
        {
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");

            Assert.Equal("203.0.113.7", RateLimitingExtensions.GetPartitionKey(context));
        }

        [Fact]
        public void GetPartitionKey_AnonymousWithoutIp_UsesSharedKey()
        {
            Assert.Equal("anonymous", RateLimitingExtensions.GetPartitionKey(new DefaultHttpContext()));
        }

        [Fact]
        public void Policy_AllowsThreeRefreshesEveryTenMinutes()
        {
            Assert.Equal(3, RateLimitingExtensions.AiPermitLimit);
            Assert.Equal(TimeSpan.FromMinutes(10), RateLimitingExtensions.AiWindow);
        }
    }
}
