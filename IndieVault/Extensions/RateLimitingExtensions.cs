using System.Security.Claims;
using System.Threading.RateLimiting;

namespace IndieVault.Extensions
{
    public static class RateLimitPolicies
    {
        public const string AiRecommendations = "AiRecommendations";
    }

    public static class RateLimitingExtensions
    {
        // Each player can ask for fresh AI recommendations 3 times every 10 minutes
        public const int AiPermitLimit = 3;
        public static readonly TimeSpan AiWindow = TimeSpan.FromMinutes(10);

        public static IServiceCollection AddAiRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

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
