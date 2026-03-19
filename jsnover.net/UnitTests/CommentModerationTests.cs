using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using jsnover.net.blazor.Models;

namespace UnitTests
{
    /// <summary>
    /// Integration tests for photo comment moderation operations.
    /// Tests comment submission, approval, rejection, and moderation workflows.
    /// </summary>
    [TestFixture]
    public class CommentModerationTests
    {
        private DbContextOptions<jsnoverdotnetdbContext> _options;

        [SetUp]
        public void SetUp()
        {
            _options = new DbContextOptionsBuilder<jsnoverdotnetdbContext>()
                .UseInMemoryDatabase(databaseName: $"CommentModerationDb_{Guid.NewGuid()}")
                .Options;

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.Database.EnsureCreated();
            }
        }

        #region Add Comment Tests

        [Test]
        public async Task AddComment_WithAllFields_CreatesUnverifiedComment()
        {
            // Arrange
            // First create a photo
            var photo = new StandalonePhoto
            {
                Title = "Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act - Add comment
            var comment = new PhotoComment
            {
                PhotoId = photoId,
                Email = "user@example.com",
                Name = "Test User",
                Message = "Great photo!",
                SubmitDate = DateTime.Now,
                IsVerified = false,
                IsApproved = false
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.PhotoComment.Add(comment);
                await context.SaveChangesAsync();
            }

            // Act - Verify comment is created
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var savedComment = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.Email == "user@example.com");

                // Assert
                Assert.That(savedComment, Is.Not.Null);
                Assert.That(savedComment.IsVerified, Is.False);
                Assert.That(savedComment.IsApproved, Is.False);
                Assert.That(savedComment.Message, Is.EqualTo("Great photo!"));
            }
        }

        [Test]
        public async Task AddComment_WithoutVerification_StatusIsUnverified()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Verification Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var photoId = photo.PhotoId;

            // Act
            var comment = new PhotoComment
            {
                PhotoId = photoId,
                Email = "unverified@example.com",
                Name = "Unverified User",
                Message = "Unverified comment",
                SubmitDate = DateTime.Now,
                IsVerified = false,
                IsApproved = false
            };

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.PhotoComment.Add(comment);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var savedComment = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.Email == "unverified@example.com");

                Assert.That(savedComment.IsVerified, Is.False);
            }
        }

        #endregion

        #region Approve Comment Tests

        [Test]
        public async Task ApproveComment_UnapprovedComment_BecomesApproved()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Approval Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var comment = new PhotoComment
            {
                Email = "user@example.com",
                Name = "User",
                Message = "Nice photo",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            };

            photo.Comments.Add(comment);

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var commentId = comment.CommentId;

            // Act - Approve comment
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var commentToApprove = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                commentToApprove.IsApproved = true;
                context.PhotoComment.Update(commentToApprove);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var approvedComment = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                Assert.That(approvedComment.IsApproved, Is.True);
            }
        }

        [Test]
        public async Task ApproveComment_MultipleComments_OnlyApprovesSelected()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Multi-Approval Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var comment1 = new PhotoComment
            {
                Email = "user1@example.com",
                Name = "User 1",
                Message = "Comment 1",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            };

            var comment2 = new PhotoComment
            {
                Email = "user2@example.com",
                Name = "User 2",
                Message = "Comment 2",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            };

            photo.Comments.Add(comment1);
            photo.Comments.Add(comment2);

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var comment1Id = comment1.CommentId;

            // Act - Approve only comment 1
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var commentToApprove = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.CommentId == comment1Id);

                commentToApprove.IsApproved = true;
                context.PhotoComment.Update(commentToApprove);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var approvedComments = await context.PhotoComment
                    .Where(c => c.IsApproved)
                    .ToListAsync();

                Assert.That(approvedComments.Count, Is.EqualTo(1));
                Assert.That(approvedComments.First().Message, Is.EqualTo("Comment 1"));
            }
        }

        #endregion

        #region Reject/Delete Comment Tests

        [Test]
        public async Task RejectComment_DeletesCommentFromApprovalQueue()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Rejection Test Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var comment = new PhotoComment
            {
                Email = "spam@example.com",
                Name = "Spammer",
                Message = "Buy now!",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            };

            photo.Comments.Add(comment);

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            var commentId = comment.CommentId;

            // Act - Reject/Delete comment
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var commentToReject = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                context.PhotoComment.Remove(commentToReject);
                await context.SaveChangesAsync();
            }

            // Assert
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var rejectedComment = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.CommentId == commentId);

                Assert.That(rejectedComment, Is.Null);
            }
        }

        #endregion

        #region Moderation List Tests

        [Test]
        public async Task GetUnapprovedComments_ReturnsOnlyUnapproved()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Moderation List Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            // Add mix of approved and unapproved
            photo.Comments.Add(new PhotoComment
            {
                Email = "approved@example.com",
                Name = "Approved User",
                Message = "Good comment",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = true
            });

            photo.Comments.Add(new PhotoComment
            {
                Email = "pending1@example.com",
                Name = "Pending User 1",
                Message = "Pending comment 1",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            });

            photo.Comments.Add(new PhotoComment
            {
                Email = "pending2@example.com",
                Name = "Pending User 2",
                Message = "Pending comment 2",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            });

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            // Act - Get unapproved comments
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var unapprovedComments = await context.PhotoComment
                    .Where(c => !c.IsApproved)
                    .ToListAsync();

                // Assert
                Assert.That(unapprovedComments.Count, Is.EqualTo(2));
                Assert.That(unapprovedComments.All(c => !c.IsApproved), Is.True);
            }
        }

        [Test]
        public async Task GetModerationQueue_FilteredByPhoto_ReturnsPhotoComments()
        {
            // Arrange
            var photo1 = new StandalonePhoto
            {
                Title = "Photo 1",
                Url = "https://example.com/photo1.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var photo2 = new StandalonePhoto
            {
                Title = "Photo 2",
                Url = "https://example.com/photo2.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            photo1.Comments.Add(new PhotoComment
            {
                Email = "user1@example.com",
                Name = "User 1",
                Message = "Comment on photo 1",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            });

            photo2.Comments.Add(new PhotoComment
            {
                Email = "user2@example.com",
                Name = "User 2",
                Message = "Comment on photo 2",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            });

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo1);
                context.StandalonePhoto.Add(photo2);
                await context.SaveChangesAsync();
            }

            var photo1Id = photo1.PhotoId;

            // Act - Get comments for photo1
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var photo1Comments = await context.PhotoComment
                    .Where(c => c.PhotoId == photo1Id && !c.IsApproved)
                    .ToListAsync();

                // Assert
                Assert.That(photo1Comments.Count, Is.EqualTo(1));
                Assert.That(photo1Comments.First().Message, Is.EqualTo("Comment on photo 1"));
            }
        }

        [Test]
        public async Task GetModerationQueue_WithoutFilter_ReturnsAllUnapproved()
        {
            // Arrange
            var photo1 = new StandalonePhoto
            {
                Title = "Photo 1",
                Url = "https://example.com/photo1.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var photo2 = new StandalonePhoto
            {
                Title = "Photo 2",
                Url = "https://example.com/photo2.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            photo1.Comments.Add(new PhotoComment
            {
                Email = "user1@example.com",
                Name = "User 1",
                Message = "Comment 1",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            });

            photo2.Comments.Add(new PhotoComment
            {
                Email = "user2@example.com",
                Name = "User 2",
                Message = "Comment 2",
                SubmitDate = DateTime.Now,
                IsVerified = true,
                IsApproved = false
            });

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo1);
                context.StandalonePhoto.Add(photo2);
                await context.SaveChangesAsync();
            }

            // Act - Get all unapproved comments
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var allUnapproved = await context.PhotoComment
                    .Where(c => !c.IsApproved)
                    .ToListAsync();

                // Assert
                Assert.That(allUnapproved.Count, Is.EqualTo(2));
            }
        }

        #endregion

        #region Verification Status Tests

        [Test]
        public async Task Comment_VerificationCode_IsStored()
        {
            // Arrange
            var photo = new StandalonePhoto
            {
                Title = "Verification Code Photo",
                Url = "https://example.com/photo.jpg",
                UploadDate = DateTime.Now,
                CreatedDate = DateTime.Now,
                IsPublished = true
            };

            var verificationCode = Guid.NewGuid().ToString();
            var verificationExpiry = DateTime.UtcNow.AddHours(24);

            var comment = new PhotoComment
            {
                Email = "verify@example.com",
                Name = "Verification User",
                Message = "Need to verify",
                SubmitDate = DateTime.Now,
                IsVerified = false,
                IsApproved = false,
                VerificationCode = verificationCode,
                VerificationCodeExpiry = verificationExpiry
            };

            photo.Comments.Add(comment);

            using (var context = new jsnoverdotnetdbContext(_options))
            {
                context.StandalonePhoto.Add(photo);
                await context.SaveChangesAsync();
            }

            // Act & Assert
            using (var context = new jsnoverdotnetdbContext(_options))
            {
                var savedComment = await context.PhotoComment
                    .FirstOrDefaultAsync(c => c.Email == "verify@example.com");

                Assert.That(savedComment.VerificationCode, Is.EqualTo(verificationCode));
                Assert.That(savedComment.VerificationCodeExpiry, Is.EqualTo(verificationExpiry));
            }
        }

        #endregion
    }
}
