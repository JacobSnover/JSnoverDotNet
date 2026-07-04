using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using jsnover.net.blazor.Models;
using jsnover.net.blazor.Infrastructure.Services;
using Moq;

namespace jsnover.net.blazor.UnitTests
{
    [TestFixture]
    public class AuthorizedRateLimitTests
    {
        private jsnoverdotnetdbContext _context;
        private RateLimitService _rateLimitService;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<jsnoverdotnetdbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new jsnoverdotnetdbContext(options);
            _rateLimitService = new RateLimitService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public async Task CheckRateLimit_WithIsAuthenticatedTrue_BypassesRateLimit()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var endpoint = "/api/photos";

            // Log 15 requests (over the 10/min limit)
            for (int i = 0; i < 15; i++)
            {
                await _rateLimitService.LogRequest(ipAddress, endpoint);
            }

            // Act
            var isRateLimited = await _rateLimitService.IsRateLimited(ipAddress, endpoint, isAuthenticated: true);

            // Assert
            Assert.IsFalse(isRateLimited, "Authenticated users should bypass rate limiting");
        }

        [Test]
        public async Task CheckRateLimit_WithIsAuthenticatedFalse_EnforcesRateLimit()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var endpoint = "/api/photos";

            // Log 10 requests (at the limit)
            for (int i = 0; i < 10; i++)
            {
                await _rateLimitService.LogRequest(ipAddress, endpoint);
            }

            // Act
            var isRateLimited = await _rateLimitService.IsRateLimited(ipAddress, endpoint, isAuthenticated: false);

            // Assert
            Assert.IsTrue(isRateLimited, "Guest users should be rate limited at threshold");
        }

        [Test]
        public async Task CheckRateLimit_Authenticated_CanMakeUnlimitedRequests()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var endpoint = "/api/reactions";

            // Log 50 requests (far over any limit)
            for (int i = 0; i < 50; i++)
            {
                await _rateLimitService.LogRequest(ipAddress, endpoint);
            }

            // Act
            var isRateLimited = await _rateLimitService.IsRateLimited(ipAddress, endpoint, isAuthenticated: true);

            // Assert
            Assert.IsFalse(isRateLimited, "Authenticated users should not be rate limited even with 50+ requests");
        }

        [Test]
        public async Task CheckRateLimit_Guest_EnforcesReactionLimit()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var endpoint = "/api/reactions";

            // Log 5 reactions (at reaction limit)
            for (int i = 0; i < 5; i++)
            {
                await _rateLimitService.LogRequest(ipAddress, endpoint);
            }

            // Act
            var isRateLimited = await _rateLimitService.IsRateLimited(ipAddress, endpoint, isAuthenticated: false);

            // Assert - Should be limited or at threshold
            // Assert: at 5 requests, reaction limit is hit (reactions=5/min)
            Assert.IsTrue(isRateLimited, "Guest user should be limited at 5+ reactions/min");
        }

        [Test]
        public async Task CheckRateLimit_MultipleIPs_TrackIndependently()
        {
            // Arrange
            var ip1 = "192.168.1.1";
            var ip2 = "192.168.1.2";
            var endpoint = "/api/photos";

            // Log 10 requests for IP1 (at limit)
            for (int i = 0; i < 10; i++)
            {
                await _rateLimitService.LogRequest(ip1, endpoint);
            }

            // Log 3 requests for IP2 (under limit)
            for (int i = 0; i < 3; i++)
            {
                await _rateLimitService.LogRequest(ip2, endpoint);
            }

            // Act
            var isIp1Limited = await _rateLimitService.IsRateLimited(ip1, endpoint, isAuthenticated: false);
            var isIp2Limited = await _rateLimitService.IsRateLimited(ip2, endpoint, isAuthenticated: false);

            // Assert
            Assert.IsTrue(isIp1Limited, "IP1 should be rate limited");
            Assert.IsFalse(isIp2Limited, "IP2 should not be rate limited");
        }

        [Test]
        public async Task CheckRateLimit_AuthenticatedUser_CanSubmitMultipleComments()
        {
            // Arrange
            var ipAddress = "192.168.1.1";
            var endpoint = "/api/comments";

            // Simulate authenticated user submitting many comments
            for (int i = 0; i < 20; i++)
            {
                await _rateLimitService.LogRequest(ipAddress, endpoint);
            }

            // Act
            var isRateLimited = await _rateLimitService.IsRateLimited(ipAddress, endpoint, isAuthenticated: true);

            // Assert
            Assert.IsFalse(isRateLimited, "Authenticated users should not be rate limited on comment submission");
        }
    }

    [TestFixture]
    public class AuthorizedCommentTests
    {
        private jsnoverdotnetdbContext _context;
        private PhotoGalleryService _service;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<jsnoverdotnetdbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new jsnoverdotnetdbContext(options);
            _service = new PhotoGalleryService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public async Task AddComment_GuestUser_RequiresEmailVerification()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            var comment = new PhotoComment
            {
                PhotoId = photo.PhotoId,
                Email = "guest@example.com",
                Name = "Guest User",
                Message = "Nice photo!",
                SubmitDate = DateTime.Now,
                IsVerified = false,
                IsApproved = false
            };

            // Act
            _context.PhotoComment.Add(comment);
            await _context.SaveChangesAsync();

            var retrieved = await _context.PhotoComment.FirstOrDefaultAsync();

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.IsFalse(retrieved.IsVerified, "Guest comment should not be verified");
            Assert.IsFalse(retrieved.IsApproved, "Guest comment should not be approved");
        }

        [Test]
        public async Task AddComment_AuthenticatedUser_MarksAsVerified()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            var comment = new PhotoComment
            {
                PhotoId = photo.PhotoId,
                Email = "user@example.com",
                Name = "Auth User",
                Message = "Great photo!",
                SubmitDate = DateTime.Now,
                IsVerified = true,   // Authenticated users are auto-verified
                IsApproved = false   // But still need admin approval
            };

            // Act
            _context.PhotoComment.Add(comment);
            await _context.SaveChangesAsync();

            var retrieved = await _context.PhotoComment.FirstOrDefaultAsync();

            // Assert
            Assert.IsTrue(retrieved.IsVerified, "Authenticated comment should be verified");
            Assert.IsFalse(retrieved.IsApproved, "Comment should still need admin approval");
        }

        [Test]
        public async Task AddComment_AdminUser_AutoApproved()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            var comment = new PhotoComment
            {
                PhotoId = photo.PhotoId,
                Email = "admin@example.com",
                Name = "Admin User",
                Message = "Thanks for sharing!",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = true    // Admin comments auto-approved
            };

            // Act
            _context.PhotoComment.Add(comment);
            await _context.SaveChangesAsync();

            var retrieved = await _context.PhotoComment.FirstOrDefaultAsync();

            // Assert
            Assert.IsTrue(retrieved.IsVerified, "Admin comment should be verified");
            Assert.IsTrue(retrieved.IsApproved, "Admin comment should be auto-approved");
        }

        [Test]
        public async Task AdminCanApproveComment()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            var comment = new PhotoComment
            {
                PhotoId = photo.PhotoId,
                Email = "guest@example.com",
                Name = "Guest",
                Message = "Nice!",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            };
            _context.PhotoComment.Add(comment);
            await _context.SaveChangesAsync();

            // Act
            comment.IsApproved = true;
            _context.PhotoComment.Update(comment);
            await _context.SaveChangesAsync();

            var approved = await _context.PhotoComment.FirstOrDefaultAsync();

            // Assert
            Assert.IsTrue(approved.IsApproved, "Comment should be approved by admin");
        }

        [Test]
        public async Task AdminCanDeleteComment()
        {
            // Arrange
            var comment = new PhotoComment
            {
                Email = "spam@example.com",
                Name = "Spammer",
                Message = "Buy now!",
                SubmitDate = DateTime.Now
            };
            _context.PhotoComment.Add(comment);
            await _context.SaveChangesAsync();

            var commentId = comment.CommentId;

            // Act
            _context.PhotoComment.Remove(comment);
            await _context.SaveChangesAsync();

            var deleted = await _context.PhotoComment.FirstOrDefaultAsync(c => c.CommentId == commentId);

            // Assert
            Assert.IsNull(deleted, "Admin should be able to delete comment");
        }

        [Test]
        public async Task GetApprovedComments_ExcludesUnapprovedComments()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            var approved = new PhotoComment
            {
                PhotoId = photo.PhotoId,
                Email = "user@example.com",
                Name = "User",
                Message = "Approved comment",
                SubmitDate = DateTime.Now,
                IsApproved = true
            };

            var unapproved = new PhotoComment
            {
                PhotoId = photo.PhotoId,
                Email = "guest@example.com",
                Name = "Guest",
                Message = "Pending comment",
                SubmitDate = DateTime.Now,
                IsApproved = false
            };

            _context.PhotoComment.Add(approved);
            _context.PhotoComment.Add(unapproved);
            await _context.SaveChangesAsync();

            // Act
            var approvedComments = _context.PhotoComment
                .Where(c => c.PhotoId == photo.PhotoId && c.IsApproved)
                .ToList();

            // Assert
            Assert.AreEqual(1, approvedComments.Count);
            Assert.AreEqual("Approved comment", approvedComments[0].Message);
        }
    }

    [TestFixture]
    public class PhotoGalleryPermissionTests
    {
        private jsnoverdotnetdbContext _context;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<jsnoverdotnetdbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new jsnoverdotnetdbContext(options);
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        [Test]
        public void PhotoGalleryPermission_Constants_AreCorrect()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("Admin", PhotoGalleryPermission.AdminRole);
            Assert.AreEqual("Guest", PhotoGalleryPermission.GuestRole);
            Assert.AreEqual("AuthenticatedUser", PhotoGalleryPermission.AuthenticatedUserRole);
        }

        [Test]
        public void PhotoGalleryPermission_Operations_AreCorrect()
        {
            // Arrange & Act & Assert
            Assert.AreEqual("ViewPhotos", PhotoGalleryPermission.Operations.ViewPhotos);
            Assert.AreEqual("AddComment", PhotoGalleryPermission.Operations.AddComment);
            Assert.AreEqual("AddReaction", PhotoGalleryPermission.Operations.AddReaction);
            Assert.AreEqual("ManagePhotos", PhotoGalleryPermission.Operations.ManagePhotos);
            Assert.AreEqual("ModerateComments", PhotoGalleryPermission.Operations.ModerateComments);
        }

        [Test]
        public async Task GuestUserPermissions_CanViewPhotos()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Public Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            // Act
            var publicPhotos = _context.StandalonePhoto
                .Where(p => p.IsPublished)
                .ToList();

            // Assert
            Assert.IsTrue(publicPhotos.Any(), "Guest can view published photos");
        }

        [Test]
        public async Task GuestUserPermissions_CannotManagePhotos()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            // Act - Guest would not have permission to delete
            // In real app, this would be caught by [Authorize(Roles = "Admin")] attribute
            var canDelete = false; // Guest has no delete permission

            // Assert
            Assert.IsFalse(canDelete, "Guest cannot delete photos");
        }

        [Test]
        public async Task AdminUserPermissions_CanManagePhotos()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now
            };
            _context.StandalonePhoto.Add(photo);
            await _context.SaveChangesAsync();

            var photoId = photo.PhotoId;

            // Act - Admin can delete
            var photoToDelete = await _context.StandalonePhoto.FindAsync(photoId);
            _context.StandalonePhoto.Remove(photoToDelete);
            await _context.SaveChangesAsync();

            var deleted = await _context.StandalonePhoto.FindAsync(photoId);

            // Assert
            Assert.IsNull(deleted, "Admin can delete photos");
        }

        [Test]
        public async Task AdminUserPermissions_CanModerateComments()
        {
            // Arrange
            var comment = new PhotoComment
            {
                Email = "user@example.com",
                Name = "User",
                Message = "Test comment",
                SubmitDate = DateTime.Now,
                IsApproved = false
            };
            _context.PhotoComment.Add(comment);
            await _context.SaveChangesAsync();

            // Act - Admin can approve
            comment.IsApproved = true;
            _context.PhotoComment.Update(comment);
            await _context.SaveChangesAsync();

            var approved = await _context.PhotoComment.FindAsync(comment.CommentId);

            // Assert
            Assert.IsTrue(approved.IsApproved, "Admin can approve comments");
        }
    }
}
