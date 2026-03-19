using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using jsnover.net.blazor.Infrastructure.Services;

namespace jsnover.net.blazor.Infrastructure.Middleware
{
    /// <summary>
    /// Middleware to enforce rate limiting on image, reaction, and comment endpoints.
    /// Extracts IP address and checks RateLimitService before allowing requests.
    /// Returns 429 Too Many Requests if rate limit exceeded.
    /// </summary>
    public class RateLimitMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimitService _rateLimitService;
        
        // Endpoints requiring rate limiting
        private static readonly string[] RateLimitedEndpoints = 
        {
            "/api/photos",
            "/api/reactions",
            "/api/comments"
        };

        public RateLimitMiddleware(RequestDelegate next, RateLimitService rateLimitService)
        {
            _next = next;
            _rateLimitService = rateLimitService;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Check if this request is for a rate-limited endpoint
                if (ShouldRateLimit(context.Request.Path))
                {
                    // Extract IP address from HttpContext
                    string ipAddress = ExtractIpAddress(context);

                    // Check if user is authenticated
                    bool isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

                    // Check rate limit
                    bool isRateLimited = await _rateLimitService.IsRateLimited(ipAddress, context.Request.Path, isAuthenticated);

                    if (isRateLimited)
                    {
                        // Return 429 Too Many Requests
                        context.Response.StatusCode = 429;
                        context.Response.ContentType = "application/json";
                        
                        // Add rate limit headers
                        context.Response.Headers.Add("X-Rate-Limit-Exceeded", "true");
                        context.Response.Headers.Add("Retry-After", "60");
                        context.Response.Headers.Add("X-Rate-Limit-Reset", DateTime.UtcNow.AddMinutes(1).ToString("o"));
                        
                        await context.Response.WriteAsync(
                            "{\"error\":\"Rate limit exceeded. Please try again later.\",\"retryAfter\":60}");
                        return;
                    }

                    // Log successful request
                    await _rateLimitService.LogRequest(ipAddress, context.Request.Path);
                }

                await _next(context);
            }
            catch (Exception)
            {
                // On error, allow the request (fail open for security middleware)
                await _next(context);
            }
        }

        /// <summary>
        /// Determines if the current request path requires rate limiting.
        /// </summary>
        private bool ShouldRateLimit(PathString path)
        {
            string pathValue = path.Value?.ToLower() ?? string.Empty;
            
            foreach (var endpoint in RateLimitedEndpoints)
            {
                if (pathValue.StartsWith(endpoint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>
        /// Extracts the client IP address from the HttpContext.
        /// Handles X-Forwarded-For header and proxy scenarios.
        /// </summary>
        private string ExtractIpAddress(HttpContext context)
        {
            try
            {
                // Check for X-Forwarded-For header (proxy/load balancer)
                if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                {
                    var ips = forwardedFor.ToString().Split(',');
                    if (ips.Length > 0 && !string.IsNullOrWhiteSpace(ips[0]))
                    {
                        return ips[0].Trim();
                    }
                }

                // Check for X-Real-IP header
                if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
                {
                    return realIp.ToString().Trim();
                }

                // Fall back to RemoteIpAddress
                return context.Connection?.RemoteIpAddress?.ToString() ?? "UNKNOWN";
            }
            catch
            {
                return "UNKNOWN";
            }
        }
    }
}
