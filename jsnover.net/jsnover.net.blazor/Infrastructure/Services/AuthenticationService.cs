using jsnover.net.blazor.Data;
using jsnover.net.blazor.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service for managing authentication state and checking user privileges.
    /// </summary>
    public class AuthenticationService
    {
        private readonly ApplicationDbContext _identityDb;
        private readonly jsnoverdotnetdbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        /// <summary>
        /// Initializes a new instance of AuthenticationService.
        /// </summary>
        /// <param name="identityDb">The identity database context</param>
        /// <param name="db">The application database context</param>
        /// <param name="userManager">The user manager for Identity operations</param>
        /// <param name="roleManager">The role manager for role operations</param>
        public AuthenticationService(
            ApplicationDbContext identityDb,
            jsnoverdotnetdbContext db,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _identityDb = identityDb ?? throw new ArgumentNullException(nameof(identityDb));
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }

        /// <summary>
        /// Gets the current authenticated user ID from the HttpContext.
        /// </summary>
        /// <param name="context">The HTTP context containing user claims</param>
        /// <returns>The user ID if authenticated; null otherwise</returns>
        public Task<string> GetCurrentUserIdAsync(HttpContext context)
        {
            try
            {
                var userId = context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return Task.FromResult(userId);
            }
            catch (Exception)
            {
                return Task.FromResult<string>(null);
            }
        }

        /// <summary>
        /// Gets the current authenticated user's email from the HttpContext.
        /// </summary>
        /// <param name="context">The HTTP context containing user claims</param>
        /// <returns>The user email if authenticated; null otherwise</returns>
        public Task<string> GetCurrentUserEmailAsync(HttpContext context)
        {
            try
            {
                var email = context?.User?.FindFirst(ClaimTypes.Email)?.Value;
                return Task.FromResult(email);
            }
            catch (Exception)
            {
                return Task.FromResult<string>(null);
            }
        }

        /// <summary>
        /// Checks if the current request is from an authenticated user.
        /// </summary>
        /// <param name="context">The HTTP context</param>
        /// <returns>True if user is authenticated; false otherwise</returns>
        public Task<bool> IsAuthenticatedAsync(HttpContext context)
        {
            try
            {
                var isAuthenticated = context?.User?.Identity?.IsAuthenticated ?? false;
                return Task.FromResult(isAuthenticated);
            }
            catch (Exception)
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Checks if a specific user is authenticated (exists in the system).
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>True if user exists; false otherwise</returns>
        public async Task<bool> IsAuthenticatedUserAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
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
        /// Checks if a user has the Admin role.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>True if user is an admin; false otherwise</returns>
        public async Task<bool> IsAdminAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.IsInRoleAsync(user, PhotoGalleryPermission.AdminRole);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a user has the AuthenticatedUser role.
        /// </summary>
        /// <param name="userId">The user ID to check</param>
        /// <returns>True if user is an authenticated user; false otherwise</returns>
        public async Task<bool> IsAuthenticatedUserRoleAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return false;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.IsInRoleAsync(user, PhotoGalleryPermission.AuthenticatedUserRole);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all comments submitted by a specific user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of comments submitted by the user</returns>
        public async Task<List<PhotoComment>> GetUserCommentsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<PhotoComment>();

                var comments = await _db.PhotoComment
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.SubmitDate)
                    .ToListAsync();

                return comments ?? new List<PhotoComment>();
            }
            catch (Exception)
            {
                return new List<PhotoComment>();
            }
        }

        /// <summary>
        /// Gets all reactions submitted by a specific user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of reactions submitted by the user</returns>
        public async Task<List<PhotoReaction>> GetUserReactionsAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<PhotoReaction>();

                var reactions = await _db.PhotoReaction
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                return reactions ?? new List<PhotoReaction>();
            }
            catch (Exception)
            {
                return new List<PhotoReaction>();
            }
        }

        /// <summary>
        /// Gets the display name for a user (email or user name).
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>The display name (email or user name); null if user not found</returns>
        public async Task<string> GetUserDisplayNameAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return null;

                var user = await _userManager.FindByIdAsync(userId);
                return user?.Email ?? user?.UserName;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if a user has a specific role.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="roleName">The role name to check</param>
        /// <returns>True if user is in the role; false otherwise</returns>
        public async Task<bool> IsUserInRoleAsync(string userId, string roleName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleName))
                    return false;

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return false;

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all roles for a user.
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of role names for the user</returns>
        public async Task<List<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<string>();

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return new List<string>();

                var roles = await _userManager.GetRolesAsync(user);
                return roles?.ToList() ?? new List<string>();
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
