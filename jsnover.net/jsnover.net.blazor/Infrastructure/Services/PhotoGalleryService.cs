using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service for managing photo gallery operations including carousel, gallery browsing, details, and search.
    /// </summary>
    public class PhotoGalleryService
    {
        private readonly jsnoverdotnetdbContext _db;

        /// <summary>
        /// Initializes a new instance of PhotoGalleryService with database context via dependency injection.
        /// </summary>
        /// <param name="db">The database context instance</param>
        public PhotoGalleryService(jsnoverdotnetdbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Retrieves random published standalone photos for carousel display.
        /// </summary>
        /// <param name="count">Number of photos to return (default 5)</param>
        /// <returns>List of random published standalone photos</returns>
        public async Task<List<StandalonePhoto>> GetCarouselPhotos(int count = 5)
        {
            try
            {
                var photos = await _db.StandalonePhoto
                    .Where(p => p.IsPublished)
                    .OrderBy(p => Guid.NewGuid())
                    .Take(count)
                    .ToListAsync();

                return photos ?? new List<StandalonePhoto>();
            }
            catch (Exception)
            {
                return new List<StandalonePhoto>();
            }
        }

        /// <summary>
        /// Retrieves gallery photos with filtering and pagination.
        /// </summary>
        /// <param name="filterBy">Filter type: "all", "blog", or "standalone" (default "all")</param>
        /// <param name="page">Page number (default 1)</param>
        /// <param name="pageSize">Number of items per page (default 20)</param>
        /// <returns>Paginated list of photos matching the filter</returns>
        public async Task<List<StandalonePhoto>> GetGalleryPhotos(string filterBy = "all", int page = 1, int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                var query = _db.StandalonePhoto.Where(p => p.IsPublished);

                // Apply filter
                if (filterBy?.ToLower() == "standalone")
                {
                    query = query.Where(p => p.PhotoId > 0);
                }
                else if (filterBy?.ToLower() == "blog")
                {
                    // Filter for blog-associated photos if needed
                    // This depends on your schema relationship
                    query = query.Where(p => p.PhotoId > 0);
                }
                // "all" includes everything

                var photos = await query
                    .OrderByDescending(p => p.UploadDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Include(p => p.Reactions)
                    .ToListAsync();

                return photos ?? new List<StandalonePhoto>();
            }
            catch (Exception)
            {
                return new List<StandalonePhoto>();
            }
        }

        /// <summary>
        /// Retrieves detailed information about a specific photo including approved comments and reaction counts.
        /// </summary>
        /// <param name="photoId">The ID of the photo to retrieve</param>
        /// <returns>Photo with comments and reactions, or null if not found</returns>
        public async Task<dynamic> GetPhotoDetail(int photoId)
        {
            try
            {
                var photo = await _db.StandalonePhoto
                    .Include(p => p.Comments.Where(c => c.IsApproved))
                    .Include(p => p.Reactions)
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId && p.IsPublished);

                if (photo == null)
                    return null;

                // Return enriched object with reaction counts
                return new
                {
                    Photo = photo,
                    Comments = photo.Comments?.ToList() ?? new List<PhotoComment>(),
                    ReactionCounts = photo.Reactions?
                        .GroupBy(r => r.ReactionType)
                        .ToDictionary(g => g.Key, g => g.Count()) ?? new Dictionary<string, int>()
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Searches photos by title, description, and tags.
        /// </summary>
        /// <param name="query">Search query string</param>
        /// <returns>List of photos matching the search criteria</returns>
        public async Task<List<StandalonePhoto>> SearchPhotos(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                    return new List<StandalonePhoto>();

                var searchTerm = query.ToLower().Trim();

                var photos = await _db.StandalonePhoto
                    .Where(p => p.IsPublished && (
                        p.Title.ToLower().Contains(searchTerm) ||
                        p.Description.ToLower().Contains(searchTerm) ||
                        p.Tags.ToLower().Contains(searchTerm)))
                    .OrderByDescending(p => p.UploadDate)
                    .ToListAsync();

                return photos ?? new List<StandalonePhoto>();
            }
            catch (Exception)
            {
                return new List<StandalonePhoto>();
            }
        }

        /// <summary>
        /// Retrieves all comments submitted by a specific authenticated user.
        /// </summary>
        /// <param name="userId">The user ID (from Identity system)</param>
        /// <returns>List of comments submitted by the user, ordered by most recent first</returns>
        public async Task<List<PhotoComment>> GetMyComments(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<PhotoComment>();

                var comments = await _db.PhotoComment
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Photo)
                    .Include(c => c.Blog)
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
        /// Retrieves all reactions submitted by a specific authenticated user.
        /// </summary>
        /// <param name="userId">The user ID (from Identity system)</param>
        /// <returns>List of reactions submitted by the user, ordered by most recent first</returns>
        public async Task<List<PhotoReaction>> GetMyReactions(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<PhotoReaction>();

                var reactions = await _db.PhotoReaction
                    .Where(r => r.UserId == userId)
                    .Include(r => r.Photo)
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
        /// Gets approval status details for a user's comments including verification status.
        /// Shows submission status: "Awaiting verification", "In review", "Approved", or "Rejected".
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <returns>List of comments with enhanced status information</returns>
        public async Task<List<dynamic>> GetMyCommentsWithStatus(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId))
                    return new List<dynamic>();

                var comments = await _db.PhotoComment
                    .Where(c => c.UserId == userId)
                    .Include(c => c.Photo)
                    .OrderByDescending(c => c.SubmitDate)
                    .ToListAsync();

                var result = comments.Select(c => new
                {
                    Comment = c,
                    Status = GetCommentStatus(c)
                }).ToList();

                return result.Cast<dynamic>().ToList();
            }
            catch (Exception)
            {
                return new List<dynamic>();
            }
        }

        /// <summary>
        /// Determines the current display status for a comment.
        /// </summary>
        /// <param name="comment">The photo comment</param>
        /// <returns>Human-readable status string</returns>
        private string GetCommentStatus(PhotoComment comment)
        {
            if (!comment.IsVerified)
                return "Awaiting Verification";
            if (!comment.IsApproved)
                return "In Review";
            return "Approved";
        }
    }
}
