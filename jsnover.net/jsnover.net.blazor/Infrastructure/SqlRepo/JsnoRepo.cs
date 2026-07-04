using jsnover.net.blazor.Constants;
using jsnover.net.blazor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace jsnover.net.blazor.Infrastructure.SqlRepo
{
    public static class JsnoRepo
    {
        public static async Task<bool> SubmitBlog(Blog[] blogs)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.Blog.Add(blogs[BlogIndexes.Blog]);
                await db.SaveChangesAsync();

                blogs[BlogIndexes.Photos].Photos.ToList().ForEach(photo => photo.BlogId = blogs[BlogIndexes.Blog].Id);
                await db.Photos.AddRangeAsync(blogs[BlogIndexes.Photos].Photos);
                await db.SaveChangesAsync();

                var blogTags = blogs[BlogIndexes.Tags].Tag.ToList();
                for (int i = 0; i < blogs[BlogIndexes.Tags].Tag.Count; i++)
                {
                    await db.Tag.AddAsync(new Tag
                    {
                        Id = 0,
                        BlogId = blogs[BlogIndexes.Blog].Id,
                        Name = blogTags[i].Name
                    });
                }
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        internal static async Task<bool> UpdateBlog(Blog[] blogs)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.Blog.Update(blogs[BlogIndexes.Blog]);
                await db.SaveChangesAsync();

                db.Photos.UpdateRange(blogs[BlogIndexes.Photos].Photos);
                await db.SaveChangesAsync();
                await db.Photos.AddRangeAsync(blogs[BlogIndexes.NewPhotos].Photos);
                await db.SaveChangesAsync();
                db.Photos.RemoveRange(blogs[BlogIndexes.RemovePhotos].Photos);
                await db.SaveChangesAsync();

                db.Tag.UpdateRange(blogs[BlogIndexes.Tags].Tag);
                await db.SaveChangesAsync();
                await db.Tag.AddRangeAsync(blogs[BlogIndexes.NewTags].Tag);
                await db.SaveChangesAsync();
                db.Tag.RemoveRange(blogs[BlogIndexes.RemoveTags].Tag);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async static Task<bool> SubmitComment(Commentors commentors)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                var blog = db.Blog.First(b => b.Id == commentors.BlogId);
                db.Commentors.Add(commentors);
                if (commentors.Liked)
                {
                    blog.Likes = blog.Likes is null ? 1 : (blog.Likes++);
                    db.Blog.Update(blog);
                }
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        internal async static Task<bool> AddSubscriber(string email)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();

                if (db.Subscribers.Where(sub => sub.Email == email).Count() == 0)
                {
                    db.Subscribers.Add(new Subscribers()
                    {
                        Id = 0,
                        Email = email,
                        SubscribeDate = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> SubmitContactRequest(ContactRequest contactRequest)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.ContactRequest.Add(contactRequest);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #region PhotoRepository Methods

        /// <summary>
        /// Adds a new standalone photo to the database.
        /// </summary>
        /// <param name="photo">The standalone photo to add</param>
        /// <returns>The PhotoId of the newly added photo</returns>
        public static async Task<int> AddStandalonePhoto(StandalonePhoto photo)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                if (photo == null)
                    return 0;

                photo.CreatedDate = DateTime.UtcNow;
                photo.UploadDate = DateTime.UtcNow;
                
                db.StandalonePhoto.Add(photo);
                await db.SaveChangesAsync();

                return photo.PhotoId;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Retrieves standalone photos with optional limit.
        /// </summary>
        /// <param name="limit">Optional maximum number of photos to retrieve</param>
        /// <returns>List of standalone photos</returns>
        public static async Task<List<StandalonePhoto>> GetStandalonePhotos(int? limit = null)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var query = db.StandalonePhoto.AsQueryable();
                
                if (limit.HasValue && limit > 0)
                {
                    query = query.Take(limit.Value);
                }

                var photos = await query
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
        /// Retrieves a specific standalone photo by its ID.
        /// </summary>
        /// <param name="photoId">The ID of the photo to retrieve</param>
        /// <returns>The standalone photo or null if not found</returns>
        public static async Task<StandalonePhoto> GetPhotoById(int photoId)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var photo = await db.StandalonePhoto
                    .Include(p => p.Comments)
                    .Include(p => p.Reactions)
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                return photo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Updates an existing standalone photo.
        /// </summary>
        /// <param name="photo">The photo with updated information</param>
        /// <returns>True if update was successful; false otherwise</returns>
        public static async Task<bool> UpdateStandalonePhoto(StandalonePhoto photo)
        {
            try
            {
                if (photo == null)
                    return false;

                using var db = new jsnoverdotnetdbContext();
                
                var existingPhoto = await db.StandalonePhoto.FirstOrDefaultAsync(p => p.PhotoId == photo.PhotoId);
                
                if (existingPhoto == null)
                    return false;

                existingPhoto.Title = photo.Title;
                existingPhoto.Description = photo.Description;
                existingPhoto.Url = photo.Url;
                existingPhoto.ThumbnailUrl = photo.ThumbnailUrl;
                existingPhoto.DisplayOrder = photo.DisplayOrder;
                existingPhoto.IsPublished = photo.IsPublished;
                existingPhoto.Tags = photo.Tags;

                db.StandalonePhoto.Update(existingPhoto);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Deletes a standalone photo by its ID.
        /// </summary>
        /// <param name="photoId">The ID of the photo to delete</param>
        /// <returns>True if deletion was successful; false otherwise</returns>
        public static async Task<bool> DeleteStandalonePhoto(int photoId)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var photo = await db.StandalonePhoto.FirstOrDefaultAsync(p => p.PhotoId == photoId);
                
                if (photo == null)
                    return false;

                db.StandalonePhoto.Remove(photo);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region CommentRepository Methods

        /// <summary>
        /// Adds a new photo comment to the database.
        /// </summary>
        /// <param name="comment">The photo comment to add</param>
        /// <returns>The CommentId of the newly added comment</returns>
        public static async Task<int> AddPhotoComment(PhotoComment comment)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                if (comment == null)
                    return 0;

                comment.SubmitDate = DateTime.UtcNow;
                
                db.PhotoComment.Add(comment);
                await db.SaveChangesAsync();

                return comment.CommentId;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Retrieves approved comments for a specific photo.
        /// </summary>
        /// <param name="photoId">The ID of the photo</param>
        /// <returns>List of approved photo comments</returns>
        public static async Task<List<PhotoComment>> GetApprovedComments(int photoId)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var comments = await db.PhotoComment
                    .Where(c => c.PhotoId == photoId && c.IsApproved && c.IsVerified)
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
        /// Retrieves all unapproved comments for administrative review.
        /// </summary>
        /// <returns>List of unapproved photo comments</returns>
        public static async Task<List<PhotoComment>> GetUnapprovedComments()
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var comments = await db.PhotoComment
                    .Where(c => !c.IsApproved)
                    .OrderBy(c => c.SubmitDate)
                    .ToListAsync();

                return comments ?? new List<PhotoComment>();
            }
            catch (Exception)
            {
                return new List<PhotoComment>();
            }
        }

        /// <summary>
        /// Approves a photo comment for public display.
        /// </summary>
        /// <param name="commentId">The ID of the comment to approve</param>
        /// <returns>True if approval was successful; false otherwise</returns>
        public static async Task<bool> ApproveComment(int commentId)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var comment = await db.PhotoComment.FirstOrDefaultAsync(c => c.CommentId == commentId);
                
                if (comment == null)
                    return false;

                comment.IsApproved = true;
                db.PhotoComment.Update(comment);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Rejects and removes a photo comment.
        /// </summary>
        /// <param name="commentId">The ID of the comment to reject</param>
        /// <returns>True if rejection was successful; false otherwise</returns>
        public static async Task<bool> RejectComment(int commentId)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                
                var comment = await db.PhotoComment.FirstOrDefaultAsync(c => c.CommentId == commentId);
                
                if (comment == null)
                    return false;

                db.PhotoComment.Remove(comment);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion
    }
}
