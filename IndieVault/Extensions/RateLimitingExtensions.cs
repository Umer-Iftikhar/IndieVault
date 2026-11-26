using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace IndieVault.Extensions
{
    public static class RateLimitPolicies
    {
        public const string AiRecommendations = "AiRecommendations";
        public const string RawgSync = "RawgSync";
    }

    public static class RateLimitingExtensions
    {
        // Each player can ask for fresh AI recommendations 3 times every 10 minutes
        public const int AiPermitLimit = 3;
        public static readonly TimeSpan AiWindow = TimeSpan.FromMinutes(10);

        // The RAWG sync can run once every 10 minutes, whichever admin starts it
        public const int RawgSyncPermitLimit = 1;
        public static readonly TimeSpan RawgSyncWindow = TimeSpan.FromMinutes(10);

        public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // Tell the browser (and the 429 page) how long to wait
                options.OnRejected = (context, _) =>
                {
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                    }

                    return ValueTask.CompletedTask;
                };

                // One fixed window per user, so one player cannot use up everyone's AI quota
                options.AddPolicy(RateLimitPolicies.AiRecommendations, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: GetPartitionKey(httpContext),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = AiPermitLimit,
                            Window = AiWindow,
                            QueueLimit = 0 // reject straight away instead of queuing
                        }));

                // One shared window for everybody: the limit protects our RAWG API quota, not a single user
                options.AddPolicy(RateLimitPolicies.RawgSync, _ =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: RateLimitPolicies.RawgSync,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = RawgSyncPermitLimit,
                            Window = RawgSyncWindow,
                            QueueLimit = 0
                        }));
            });

            return services;
        }

        // Signed-in users are limited by user id; anything else falls back to the IP address
        public static string GetPartitionKey(HttpContext httpContext)
        {
            return httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                ?? "anonymous";
        }
    }
}
