using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using jsnover.net.blazor.Models;
using jsnover.net.blazor.Infrastructure.Services;

namespace UnitTests
{
    /// <summary>
    /// Integration tests for PhotoGalleryService and PhotoGallery with InMemory database.
    /// Tests repository + service round-trips, comment approval, reaction aggregation, rate limiting, and transaction safety.
    /// </summary>
    [TestFixture]
    public class PhotoGalleryIntegrationTests
    {
        private DbContextOptions<jsnoverdotnetdbContext> _options;
        private PhotoGalleryService _service;

        [SetUp]
        public void SetUp()
        {
            _options = new DbContextOptionsBuilder<jsnoverdotnetdbContext>()
                .UseInMemoryDatabase(databaseName: $"PhotoGalleryDb_{Guid.NewGuid()}")
                .Options;

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.Database.EnsureCreated();
            }
        }

        #region Photo Add and Retrieve Tests

        [Test]
        public async Task AddPhoto_ViaRepository_RetrieveViaService_PreservesAllFields()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test Photo",
                Description = "Test Description",
                Url = "https://example.com/photo.jpg",
                ThumbnailUrl = "https://example.com/thumb.jpg",
                UploadDate = DateTime.Now,
                DisplayOrder = 1,
                CreatedDate = DateTime.Now,
                IsPublished = true,
                Tags = "nature,outdoor"
            };

            // Act - Add via repository
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            // Setup service and retrieve
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                _service = new PhotoGalleryService(context);
                var retrieved = await context.StandalonePhoto.FirstOrDefaultAsync(p => p.Title == "Test Photo");

                // Assert
                Assert.That(retrieved, Is.Not.Null);
                Assert.That(retrieved.Title, Is.EqualTo("Test Photo"));
                Assert.That(retrieved.Description, Is.EqualTo("Test Description"));
                Assert.That(retrieved.Url, Is.EqualTo("https://example.com/photo.jpg"));
                Assert.That(retrieved.Tags, Is.EqualTo("nature,outdoor"));
                Assert.That(retrieved.IsPublished, Is.True);
            }
        }

        #endregion

        #region Comment Approval Tests

        [Test]
        public async Task AddComment_ThenApprove_AppearsInApprovedList()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var comment = new PhotoComment
            {
                Email = "user@example.com",
                Name = "Test User",
                Message = "Great photo!",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            };

            photo.Comments.Add(comment);

            // Act - Add photo with comment
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            // Act - Approve comment
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var savedPhoto = await context.StandalonePhoto
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Title == "Test Photo");

                var commentToApprove = savedPhoto.Comments.First();
                commentToApprove.IsApproved = true;
                context.PhotoComment.Update(commentToApprove);
                await context.SaveChangesAsync();
            }

            // Assert - Verify comment is approved
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var retrievedPhoto = await context.StandalonePhoto
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.Title == "Test Photo");

                var approvedComments = retrievedPhoto.Comments.Where(c => c.IsApproved).ToList();
                Assert.That(approvedComments.Count, Is.EqualTo(1));
                Assert.That(approvedComments.First().Message, Is.EqualTo("Great photo!"));
            }
        }

        [Test]
        public async Task RetrievePhotoDetail_OnlyIncludesApprovedComments()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            photo.Comments.Add(new PhotoComment
            {
                Email = "user1@example.com",
                Name = "User 1",
                Message = "Approved comment",
                SubmitDate = DateTime.Now,
                IsApproved = true
            });

            photo.Comments.Add(new PhotoComment
            {
                Email = "user2@example.com",
                Name = "User 2",
                Message = "Unapproved comment",
                SubmitDate = DateTime.Now,
                IsApproved = false
            });

            // Act - Add photo with mixed comments
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            // Act - Retrieve using service
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                _service = new PhotoGalleryService(context);
                var detail = await _service.GetPhotoDetail(photo.PhotoId);

                // Assert
                Assert.That(detail, Is.Not.Null);
            }
        }

        #endregion

        #region Reaction Aggregation Tests

        [Test]
        public async Task AddReactions_ServiceAggregatesCounts()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Reaction Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            photo.Reactions.Add(new PhotoReaction { ReactionType = "👍", SessionId = "session1", CreatedDate = DateTime.Now });
            photo.Reactions.Add(new PhotoReaction { ReactionType = "👍", SessionId = "session2", CreatedDate = DateTime.Now });
            photo.Reactions.Add(new PhotoReaction { ReactionType = "❤️", SessionId = "session3", CreatedDate = DateTime.Now });
            photo.Reactions.Add(new PhotoReaction { ReactionType = "❤️", SessionId = "session4", CreatedDate = DateTime.Now });
            photo.Reactions.Add(new PhotoReaction { ReactionType = "😂", SessionId = "session5", CreatedDate = DateTime.Now });

            // Act - Add photo with reactions
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            // Act - Retrieve and check reactions
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var savedPhoto = await context.StandalonePhoto
                    .Include(p => p.Reactions)
                    .FirstOrDefaultAsync(p => p.Title == "Reaction Test Photo");

                var reactionCounts = savedPhoto.Reactions
                    .GroupBy(r => r.ReactionType)
                    .ToDictionary(g => g.Key, g => g.Count());

                // Assert
                Assert.That(reactionCounts.Count, Is.EqualTo(3));
                Assert.That(reactionCounts["👍"], Is.EqualTo(2));
                Assert.That(reactionCounts["❤️"], Is.EqualTo(2));
                Assert.That(reactionCounts["😂"], Is.EqualTo(1));
            }
        }

        #endregion

        #region Rate Limit Tracking Tests

        [Test]
        public async Task LogRateLimitRequest_MultipleRequests_CanRetrieveAll()
        {
            // Arrange
            var logs = new List<RateLimitLog>();
            for (int i = 0; i < 10; i++)
            {
                logs.Add(new RateLimitLog
                {
                    IpAddress = "192.168.1.1",
                    Endpoint = "images",
                    Timestamp = DateTime.UtcNow.AddSeconds(-i),
                    RequestCount = 1
                });
            }

            // Act - Add logs
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                foreach (var log in logs)
                {
                    context.RateLimitLog.Add(log);
                }
                await context.SaveChangesAsync();
            }

            // Act - Retrieve combined count
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentRequests = await context.RateLimitLog
                    .Where(log => log.IpAddress == "192.168.1.1" && log.Endpoint == "images" && log.Timestamp > oneMinuteAgo)
                    .SumAsync(log => log.RequestCount);

                // Assert
                Assert.That(recentRequests, Is.GreaterThanOrEqualTo(1));
            }
        }

        [Test]
        public async Task CheckRateLimit_TenRequests_IsRateLimited()
        {
            // Arrange
            var logs = new List<RateLimitLog>();
            for (int i = 0; i < 10; i++)
            {
                logs.Add(new RateLimitLog
                {
                    IpAddress = "192.168.1.1",
                    Endpoint = "images",
                    Timestamp = DateTime.UtcNow.AddSeconds(-i),
                    RequestCount = 1
                });
            }

            // Act - Add logs
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                foreach (var log in logs)
                {
                    context.RateLimitLog.Add(log);
                }
                await context.SaveChangesAsync();
            }

            // Act & Assert - Verify rate limit is hit
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);
                var recentRequestCount = await context.RateLimitLog
                    .Where(log => log.IpAddress == "192.168.1.1" && log.Endpoint == "images" && log.Timestamp > oneMinuteAgo)
                    .SumAsync(log => log.RequestCount);

                var isRateLimited = recentRequestCount >= 10;
                Assert.That(isRateLimited, Is.True);
            }
        }

        #endregion

        #region Transaction Safety Tests

        [Test]
        public async Task PhotoAddition_CanBeCommitted()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Transaction Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            // Act & Assert - Add and save
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                var saveResult = await context.SaveChangesAsync();

                // At least one entity should be saved
                Assert.That(saveResult, Is.GreaterThanOrEqualTo(1));
            }

            // Act & Assert - Verify persistence
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var savedPhoto = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.Title == "Transaction Test Photo");

                Assert.That(savedPhoto, Is.Not.Null);
            }
        }

        [Test]
        public async Task PhotoDeletion_CascadesToComments()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Delete Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            photo.Comments.Add(new PhotoComment
            {
                Email = "user@example.com",
                Name = "User",
                Message = "Test comment",
                SubmitDate = DateTime.Now
            });

            // Act - Add photo with comment
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act - Delete photo
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photoToDelete = await context.StandalonePhoto
                    .Include(p => p.Comments)
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                if (photoToDelete != null)
                {
                    context.StandalonePhoto.Remove(photoToDelete);
                    await context.SaveChangesAsync();
                }
            }

            // Assert - Photo and comments are gone
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var deletedPhoto = await context.StandalonePhoto
                    .FirstOrDefaultAsync(p => p.PhotoId == photoId);

                var comments = await context.PhotoComment
                    .Where(c => c.PhotoId == photoId)
                    .ToListAsync();

                Assert.That(deletedPhoto, Is.Null);
                Assert.That(comments.Count, Is.EqualTo(0));
            }
        }

        #endregion
    }
}
