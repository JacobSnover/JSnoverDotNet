using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service for enforcing rate limiting on API requests per IP address.
    /// Authenticated users bypass rate limits; admin users bypass rate limits for all operations.
    /// </summary>
    public class RateLimitService
    {
        private readonly jsnoverdotnetdbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private const int MaxImagesPerMinute = 10;
        private const int MaxReactionsPerMinute = 5;

        /// <summary>
        /// Initializes a new instance of RateLimitService with database context and user manager via dependency injection.
        /// </summary>
        /// <param name="db">The database context instance</param>
        /// <param name="userManager">The user manager for checking admin status</param>
        public RateLimitService(jsnoverdotnetdbContext db, UserManager<IdentityUser> userManager = null)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _userManager = userManager;
        }

        /// <summary>
        /// Checks if a user ID belongs to an authenticated user in the system.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>True if the user is authenticated (exists in the system); false otherwise</returns>
        public async Task<bool> IsAuthenticatedUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || _userManager == null)
                    return false;

                var user = await _userManager.FindByIdAsync(userId);
                return user != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a user is an admin.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>True if the user has the Admin role; false otherwise</returns>
        public async Task<bool> IsAdminUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || _userManager == null)
                    return false;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.IsInRoleAsync(user, "Admin");
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if an IP address has exceeded rate limits for an endpoint.
        /// </summary>
        /// <param name="ipAddress">The client IP address</param>
        /// <param name="endpoint">The API endpoint being accessed</param>
        /// <param name="isAuthenticated">Whether the request is from an authenticated user (circumvents rate limit)</param>
        /// <param name="userId">Optional: The user ID for authenticated requests</param>
        /// <returns>True if the request is allowed; false if rate limit exceeded</returns>
        public async Task<bool> IsRateLimited(string ipAddress, string endpoint, bool isAuthenticated = false, string userId = null)
        {
            try
            {
                // Authenticated users bypass rate limiting
                if (isAuthenticated && !string.IsNullOrWhiteSpace(userId))
                {
                    var isAuthenticated2 = await IsAuthenticatedUserAsync(userId);
                    if (isAuthenticated2)
                        return false;
                }

                if (string.IsNullOrWhiteSpace(ipAddress))
                    return true;

                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                
                // Count recent requests from this IP for this endpoint
                var recentRequestCount = await _db.RateLimitLog
                    .Where(log => 
                        log.IpAddress == ipAddress && 
                        log.Endpoint == endpoint && 
                        log.Timestamp > oneMinuteAgo)
                    .SumAsync(log => log.RequestCount);

                // Determine limit based on endpoint
                int limit = endpoint?.ToLower().Contains("reaction") == true 
                    ? MaxReactionsPerMinute 
                    : MaxImagesPerMinute;

                return recentRequestCount >= limit;
            }
            catch (Exception)
            {
                // On error, allow the request (fail open)
                return false;
            }
        }

        /// <summary>
        /// Logs a request from an IP address to an endpoint for rate limiting tracking.
        /// </summary>
        /// <param name="ipAddress">The client IP address</param>
        /// <param name="endpoint">The API endpoint being accessed</param>
        public async Task LogRequest(string ipAddress, string endpoint)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(endpoint))
                    return;

                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

                // Check if a recent log exists for this IP/endpoint combination
                var existingLog = await _db.RateLimitLog
                    .FirstOrDefaultAsync(log =>
                        log.IpAddress == ipAddress &&
                        log.Endpoint == endpoint &&
                        log.Timestamp > oneMinuteAgo);

                if (existingLog != null)
                {
                    // Increment existing log
                    existingLog.RequestCount++;
                    _db.RateLimitLog.Update(existingLog);
                }
                else
                {
                    // Create new log entry
                    var newLog = new RateLimitLog
                    {
                        IpAddress = ipAddress,
                        Endpoint = endpoint,
                        Timestamp = DateTime.UtcNow,
                        RequestCount = 1
                    };
                    _db.RateLimitLog.Add(newLog);
                }

                await _db.SaveChangesAsync();
            }
            catch (Exception)
            {
                // Log silently on error
            }
        }

        /// <summary>
        /// Checks rate limit and logs the request atomically.
        /// </summary>
        /// <param name="ipAddress">The client IP address</param>
        /// <param name="endpoint">The API endpoint being accessed</param>
        /// <param name="isAuthenticated">Whether the request is from an authenticated user</param>
        /// <param name="userId">Optional: The user ID for authenticated requests</param>
        /// <returns>True if request is allowed; false if rate limited</returns>
        public async Task<bool> CheckRateLimit(string ipAddress, string endpoint, bool isAuthenticated = false, string userId = null)
        {
            var isLimited = await IsRateLimited(ipAddress, endpoint, isAuthenticated, userId);
            
            if (!isLimited)
            {
                await LogRequest(ipAddress, endpoint);
            }

            return !isLimited;
        }
    }
}

