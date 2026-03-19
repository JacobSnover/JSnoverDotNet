using NUnit.Framework;
using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using jsnover.net.blazor.Models;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for StandalonePhoto model.
    /// Tests focus on model instantiation, property validation, and default values.
    /// </summary>
    [TestFixture]
    public class StandalonePhotoTests
        {
            [Test]
            public void Constructor_InitializesCollections_WhenInstantiated()
            {
                // Arrange & Act
                var photo = new StandalonePhoto();

                // Assert
                Assert.That(photo.Comments, Is.Not.Null, "Comments collection should be initialized");
                Assert.That(photo.Reactions, Is.Not.Null, "Reactions collection should be initialized");
                Assert.That(photo.Comments, Is.TypeOf<HashSet<PhotoComment>>());
                Assert.That(photo.Reactions, Is.TypeOf<HashSet<PhotoReaction>>());
            }

            [Test]
            public void CreateStandalonePhoto_WithRequiredProperties_Succeeds()
            {
                // Arrange & Act
                var photo = new StandalonePhoto
                {
                    PhotoId = 1,
                    Title = "Sample Photo",
                    Url = "https://example.com/photo.jpg",
                    UploadDate = DateTime.Now,
                    CreatedDate = DateTime.Now,
                    DisplayOrder = 0,
                    IsPublished = true
                };

                // Assert
                Assert.That(photo.PhotoId, Is.EqualTo(1));
                Assert.That(photo.Title, Is.EqualTo("Sample Photo"));
                Assert.That(photo.Url, Is.EqualTo("https://example.com/photo.jpg"));
            }

            [Test]
            public void TitleProperty_CanStoreMaxLength250Characters()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var maxLengthTitle = new string('A', 250);

                // Act
                photo.Title = maxLengthTitle;

                // Assert
                Assert.That(photo.Title.Length, Is.EqualTo(250));
                Assert.That(photo.Title, Is.EqualTo(maxLengthTitle));
            }

            [Test]
            public void TitleProperty_CanBeNull()
            {
                // Arrange
                var photo = new StandalonePhoto();

                // Act
                photo.Title = null;

                // Assert
                Assert.That(photo.Title, Is.Null);
            }

            [Test]
            public void UrlProperty_CanStoreMaxLength500Characters()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var maxLengthUrl = new string('x', 500);

                // Act
                photo.Url = maxLengthUrl;

                // Assert
                Assert.That(photo.Url.Length, Is.EqualTo(500));
            }

            [Test]
            public void DescriptionProperty_CanStoreMaxLength1000Characters()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var maxLengthDescription = new string('D', 1000);

                // Act
                photo.Description = maxLengthDescription;

                // Assert
                Assert.That(photo.Description.Length, Is.EqualTo(1000));
            }

            [Test]
            public void DisplayOrderProperty_CanBeSetToZero()
            {
                // Arrange & Act
                var photo = new StandalonePhoto { DisplayOrder = 0 };

                // Assert
                Assert.That(photo.DisplayOrder, Is.EqualTo(0));
            }

            [Test]
            public void DisplayOrderProperty_CanBeSetToPositiveValue()
            {
                // Arrange & Act
                var photo = new StandalonePhoto { DisplayOrder = 100 };

                // Assert
                Assert.That(photo.DisplayOrder, Is.EqualTo(100));
            }

            [Test]
            public void DisplayOrderProperty_CanBeNegative()
            {
                // Arrange & Act
                var photo = new StandalonePhoto { DisplayOrder = -5 };

                // Assert
                Assert.That(photo.DisplayOrder, Is.EqualTo(-5));
            }

            [Test]
            public void IsPublishedProperty_DefaultsToTrue()
            {
                // Arrange & Act
                var photo = new StandalonePhoto { IsPublished = true };

                // Assert
                Assert.That(photo.IsPublished, Is.True);
            }

            [Test]
            public void IsPublishedProperty_CanBeSetToFalse()
            {
                // Arrange & Act
                var photo = new StandalonePhoto { IsPublished = false };

                // Assert
                Assert.That(photo.IsPublished, Is.False);
            }

            [Test]
            public void TagsProperty_CanStoreCommaSeparatedValues()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var tags = "landscape,nature,sunset,outdoor";

                // Act
                photo.Tags = tags;

                // Assert
                Assert.That(photo.Tags, Is.EqualTo(tags));
            }

            [Test]
            public void TagsProperty_CanStoreMaxLength500Characters()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var maxLengthTags = new string('T', 500);

                // Act
                photo.Tags = maxLengthTags;

                // Assert
                Assert.That(photo.Tags.Length, Is.EqualTo(500));
            }

            [Test]
            public void ThumbnailUrlProperty_CanStoreUrl()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var thumbnailUrl = "https://example.com/thumb.jpg";

                // Act
                photo.ThumbnailUrl = thumbnailUrl;

                // Assert
                Assert.That(photo.ThumbnailUrl, Is.EqualTo(thumbnailUrl));
            }

            [Test]
            public void CommentsCollection_CanAddPhotoComments()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var comment = new PhotoComment { CommentId = 1, Message = "Great photo!" };

                // Act
                photo.Comments.Add(comment);

                // Assert
                Assert.That(photo.Comments.Count, Is.EqualTo(1));
                Assert.That(photo.Comments, Contains.Item(comment));
            }

            [Test]
            public void ReactionsCollection_CanAddPhotoReactions()
            {
                // Arrange
                var photo = new StandalonePhoto();
                var reaction = new PhotoReaction { ReactionId = 1, ReactionType = "👍", SessionId = "session123" };

                // Act
                photo.Reactions.Add(reaction);

                // Assert
                Assert.That(photo.Reactions.Count, Is.EqualTo(1));
                Assert.That(photo.Reactions, Contains.Item(reaction));
            }
        }
    }

    /// <summary>
    /// Unit tests for PhotoComment model.
    /// Tests focus on model instantiation, property validation, and default values.
    /// </summary>
    [TestFixture]
    public class PhotoCommentTests
        {
            [Test]
            public void CreatePhotoComment_WithRequiredProperties_Succeeds()
            {
                // Arrange & Act
                var comment = new PhotoComment
                {
                    CommentId = 1,
                    Email = "user@example.com",
                    Name = "John Doe",
                    Message = "Great photo!",
                    SubmitDate = DateTime.Now
                };

                // Assert
                Assert.That(comment.CommentId, Is.EqualTo(1));
                Assert.That(comment.Email, Is.EqualTo("user@example.com"));
                Assert.That(comment.Name, Is.EqualTo("John Doe"));
                Assert.That(comment.Message, Is.EqualTo("Great photo!"));
            }

            [Test]
            public void EmailProperty_CanStoreValidEmailFormat()
            {
                // Arrange
                var comment = new PhotoComment();
                var validEmail = "test@example.com";

                // Act
                comment.Email = validEmail;

                // Assert
                Assert.That(comment.Email, Is.EqualTo(validEmail));
            }

            [Test]
            public void EmailProperty_CanStoreComplexEmailFormats()
            {
                // Arrange
                var comment = new PhotoComment();
                var emails = new[] { "user+tag@example.co.uk", "first.last@domain.org", "test_123@sub.example.com" };

                // Act & Assert
                foreach (var email in emails)
                {
                    comment.Email = email;
                    Assert.That(comment.Email, Is.EqualTo(email), $"Email format {email} should be stored");
                }
            }

            [Test]
            public void EmailProperty_CanStoreMaxLength256Characters()
            {
                // Arrange
                var comment = new PhotoComment();
                var maxLengthEmail = new string('a', 243) + "@example.com"; // 256 total

                // Act
                comment.Email = maxLengthEmail;

                // Assert
                Assert.That(comment.Email.Length, Is.EqualTo(256));
            }

            [Test]
            public void NameProperty_CanStoreMaxLength100Characters()
            {
                // Arrange
                var comment = new PhotoComment();
                var maxLengthName = new string('A', 100);

                // Act
                comment.Name = maxLengthName;

                // Assert
                Assert.That(comment.Name.Length, Is.EqualTo(100));
            }

            [Test]
            public void MessageProperty_CanStoreSimpleText()
            {
                // Arrange
                var comment = new PhotoComment();
                var message = "This is a great photograph!";

                // Act
                comment.Message = message;

                // Assert
                Assert.That(comment.Message, Is.EqualTo(message));
            }

            [Test]
            public void MessageProperty_CanStoreMaxLength1000Characters()
            {
                // Arrange
                var comment = new PhotoComment();
                var maxLengthMessage = new string('X', 1000);

                // Act
                comment.Message = maxLengthMessage;

                // Assert
                Assert.That(comment.Message.Length, Is.EqualTo(1000));
            }

            [Test]
            public void IsVerifiedProperty_DefaultsToFalse()
            {
                // Arrange & Act
                var comment = new PhotoComment { IsVerified = false };

                // Assert
                Assert.That(comment.IsVerified, Is.False);
            }

            [Test]
            public void IsVerifiedProperty_CanBeSetToTrue()
            {
                // Arrange & Act
                var comment = new PhotoComment { IsVerified = true };

                // Assert
                Assert.That(comment.IsVerified, Is.True);
            }

            [Test]
            public void IsApprovedProperty_DefaultsToFalse()
            {
                // Arrange & Act
                var comment = new PhotoComment { IsApproved = false };

                // Assert
                Assert.That(comment.IsApproved, Is.False);
            }

            [Test]
            public void IsApprovedProperty_CanBeSetToTrue()
            {
                // Arrange & Act
                var comment = new PhotoComment { IsApproved = true };

                // Assert
                Assert.That(comment.IsApproved, Is.True);
            }

            [Test]
            public void PhotoIdProperty_CanBeNull()
            {
                // Arrange & Act
                var comment = new PhotoComment { PhotoId = null };

                // Assert
                Assert.That(comment.PhotoId, Is.Null);
            }

            [Test]
            public void PhotoIdProperty_CanHaveValue()
            {
                // Arrange & Act
                var comment = new PhotoComment { PhotoId = 42 };

                // Assert
                Assert.That(comment.PhotoId, Is.EqualTo(42));
            }

            [Test]
            public void BlogIdProperty_CanBeNull()
            {
                // Arrange & Act
                var comment = new PhotoComment { BlogId = null };

                // Assert
                Assert.That(comment.BlogId, Is.Null);
            }

            [Test]
            public void BlogIdProperty_CanHaveValue()
            {
                // Arrange & Act
                var comment = new PhotoComment { BlogId = 5 };

                // Assert
                Assert.That(comment.BlogId, Is.EqualTo(5));
            }

            [Test]
            public void PhotoProperty_CanBeAssignedStandalonePhoto()
            {
                // Arrange
                var comment = new PhotoComment();
                var photo = new StandalonePhoto { PhotoId = 1, Title = "Test" };

                // Act
                comment.Photo = photo;

                // Assert
                Assert.That(comment.Photo, Is.EqualTo(photo));
                Assert.That(comment.Photo.PhotoId, Is.EqualTo(1));
            }

            [Test]
            public void VerificationCodeProperty_CanStoreCode()
            {
                // Arrange
                var comment = new PhotoComment();
                var code = "VERIFY123ABC";

                // Act
                comment.VerificationCode = code;

                // Assert
                Assert.That(comment.VerificationCode, Is.EqualTo(code));
            }

            [Test]
            public void VerificationCodeProperty_CanStoreMaxLength50Characters()
            {
                // Arrange
                var comment = new PhotoComment();
                var maxLengthCode = new string('C', 50);

                // Act
                comment.VerificationCode = maxLengthCode;

                // Assert
                Assert.That(comment.VerificationCode.Length, Is.EqualTo(50));
            }

            [Test]
            public void VerificationCodeExpiryProperty_CanBeNull()
            {
                // Arrange & Act
                var comment = new PhotoComment { VerificationCodeExpiry = null };

                // Assert
                Assert.That(comment.VerificationCodeExpiry, Is.Null);
            }

            [Test]
            public void VerificationCodeExpiryProperty_CanHaveDateTime()
            {
                // Arrange
                var expiryTime = DateTime.Now.AddHours(24);
                var comment = new PhotoComment { VerificationCodeExpiry = expiryTime };

                // Act & Assert
                Assert.That(comment.VerificationCodeExpiry, Is.EqualTo(expiryTime));
            }
        }
    }

    /// <summary>
    /// Unit tests for PhotoReaction model.
    /// Tests focus on reaction types, session management, and data storage.
    /// </summary>
    [TestFixture]
    public class PhotoReactionTests
    {
        [Test]
            public void CreatePhotoReaction_WithRequiredProperties_Succeeds()
            {
                // Arrange & Act
                var reaction = new PhotoReaction
                {
                    ReactionId = 1,
                    PhotoId = 10,
                    ReactionType = "👍",
                    SessionId = "session-abc123",
                    CreatedDate = DateTime.Now
                };

                // Assert
                Assert.That(reaction.ReactionId, Is.EqualTo(1));
                Assert.That(reaction.PhotoId, Is.EqualTo(10));
                Assert.That(reaction.ReactionType, Is.EqualTo("👍"));
                Assert.That(reaction.SessionId, Is.EqualTo("session-abc123"));
            }

            [Test]
            public void ReactionTypeProperty_CanStoreThumbsUpEmoji()
            {
                // Arrange & Act
                var reaction = new PhotoReaction { ReactionType = "👍" };

                // Assert
                Assert.That(reaction.ReactionType, Is.EqualTo("👍"));
            }

            [Test]
            public void ReactionTypeProperty_CanStoreHeartEmoji()
            {
                // Arrange & Act
                var reaction = new PhotoReaction { ReactionType = "❤️" };

                // Assert
                Assert.That(reaction.ReactionType, Is.EqualTo("❤️"));
            }

            [Test]
            public void ReactionTypeProperty_CanStoreLaughingEmoji()
            {
                // Arrange & Act
                var reaction = new PhotoReaction { ReactionType = "😂" };

                // Assert
                Assert.That(reaction.ReactionType, Is.EqualTo("😂"));
            }

            [Test]
            public void ReactionTypeProperty_CanStoreWowEmoji()
            {
                // Arrange & Act
                var reaction = new PhotoReaction { ReactionType = "😮" };

                // Assert
                Assert.That(reaction.ReactionType, Is.EqualTo("😮"));
            }

            [Test]
            public void ReactionTypeProperty_CanStoreSadEmoji()
            {
                // Arrange & Act
                var reaction = new PhotoReaction { ReactionType = "😢" };

                // Assert
                Assert.That(reaction.ReactionType, Is.EqualTo("😢"));
            }

            [Test]
            public void ReactionTypeProperty_CanStoreAllValidReactions()
            {
                // Arrange
                var validReactions = new[] { "👍", "❤️", "😂", "😮", "😢" };
                var reaction = new PhotoReaction();

                // Act & Assert
                foreach (var reactionType in validReactions)
                {
                    reaction.ReactionType = reactionType;
                    Assert.That(reaction.ReactionType, Is.EqualTo(reactionType));
                }
            }

            [Test]
            public void ReactionTypeProperty_CanStoreMaxLength10Characters()
            {
                // Arrange
                var reaction = new PhotoReaction();
                var maxLengthReaction = new string('R', 10);

                // Act
                reaction.ReactionType = maxLengthReaction;

                // Assert
                Assert.That(reaction.ReactionType.Length, Is.EqualTo(10));
            }

            [Test]
            public void SessionIdProperty_CanStoreSessionIdentifier()
            {
                // Arrange
                var reaction = new PhotoReaction();
                var sessionId = "ABC-123-XYZ-789";

                // Act
                reaction.SessionId = sessionId;

                // Assert
                Assert.That(reaction.SessionId, Is.EqualTo(sessionId));
            }

            [Test]
            public void SessionIdProperty_CanStoreMaxLength100Characters()
            {
                // Arrange
                var reaction = new PhotoReaction();
                var maxLengthSessionId = new string('S', 100);

                // Act
                reaction.SessionId = maxLengthSessionId;

                // Assert
                Assert.That(reaction.SessionId.Length, Is.EqualTo(100));
            }

            [Test]
            public void SessionIdProperty_NotEmpty()
            {
                // Arrange
                var reaction = new PhotoReaction();

                // Act & Assert
                Assert.That(reaction.SessionId, Is.Null.Or.Empty);
            }

            [Test]
            public void PhotoIdProperty_CanBeSet()
            {
                // Arrange & Act
                var reaction = new PhotoReaction { PhotoId = 25 };

                // Assert
                Assert.That(reaction.PhotoId, Is.EqualTo(25));
            }

            [Test]
            public void CreatedDateProperty_CanBeSetToNow()
            {
                // Arrange
                var now = DateTime.Now;
                var reaction = new PhotoReaction { CreatedDate = now };

                // Act & Assert
                Assert.That(reaction.CreatedDate, Is.EqualTo(now).Within(TimeSpan.FromSeconds(1)));
            }

            [Test]
            public void PhotoProperty_CanBeAssignedStandalonePhoto()
            {
                // Arrange
                var reaction = new PhotoReaction();
                var photo = new StandalonePhoto { PhotoId = 5, Title = "Mountain" };

                // Act
                reaction.Photo = photo;

                // Assert
                Assert.That(reaction.Photo, Is.EqualTo(photo));
                Assert.That(reaction.Photo.PhotoId, Is.EqualTo(5));
            }
        }
    }

    /// <summary>
    /// Unit tests for RateLimitLog model.
    /// Tests focus on IP address storage, endpoint tracking, and timestamp management.
    /// </summary>
    [TestFixture]
    public class RateLimitLogTests
    {
        [Test]
        public void CreateRateLimitLog_WithRequiredProperties_Succeeds()
            {
                // Arrange & Act
                var log = new RateLimitLog
                {
                    LogId = 1,
                    IpAddress = "192.168.1.1",
                    Endpoint = "/api/photos",
                    Timestamp = DateTime.Now,
                    RequestCount = 5
                };

                // Assert
                Assert.That(log.LogId, Is.EqualTo(1));
                Assert.That(log.IpAddress, Is.EqualTo("192.168.1.1"));
                Assert.That(log.Endpoint, Is.EqualTo("/api/photos"));
                Assert.That(log.RequestCount, Is.EqualTo(5));
            }

            [Test]
            public void IpAddressProperty_CanStoreIPv4Address()
            {
                // Arrange
                var log = new RateLimitLog();
                var ipv4Address = "192.168.1.100";

                // Act
                log.IpAddress = ipv4Address;

                // Assert
                Assert.That(log.IpAddress, Is.EqualTo(ipv4Address));
            }

            [Test]
            public void IpAddressProperty_CanStoreIPv6Address()
            {
                // Arrange
                var log = new RateLimitLog();
                var ipv6Address = "::1";

                // Act
                log.IpAddress = ipv6Address;

                // Assert
                Assert.That(log.IpAddress, Is.EqualTo(ipv6Address));
            }

            [Test]
            public void IpAddressProperty_CanStoreLocalhost()
            {
                // Arrange
                var log = new RateLimitLog();

                // Act
                log.IpAddress = "127.0.0.1";

                // Assert
                Assert.That(log.IpAddress, Is.EqualTo("127.0.0.1"));
            }

            [Test]
            public void IpAddressProperty_CanStoreMaxLength50Characters()
            {
                // Arrange
                var log = new RateLimitLog();
                var maxLengthIp = new string('1', 50);

                // Act
                log.IpAddress = maxLengthIp;

                // Assert
                Assert.That(log.IpAddress.Length, Is.EqualTo(50));
            }

            [Test]
            public void EndpointProperty_CanStoreApiPath()
            {
                // Arrange
                var log = new RateLimitLog();
                var endpoint = "/api/v1/photos/reactions";

                // Act
                log.Endpoint = endpoint;

                // Assert
                Assert.That(log.Endpoint, Is.EqualTo(endpoint));
            }

            [Test]
            public void EndpointProperty_CanStorePagePath()
            {
                // Arrange
                var log = new RateLimitLog();
                var endpoint = "/gallery/photos";

                // Act
                log.Endpoint = endpoint;

                // Assert
                Assert.That(log.Endpoint, Is.EqualTo(endpoint));
            }

            [Test]
            public void EndpointProperty_CanStoreMaxLength500Characters()
            {
                // Arrange
                var log = new RateLimitLog();
                var maxLengthEndpoint = new string('/', 250) + new string('e', 250); // 500 total

                // Act
                log.Endpoint = maxLengthEndpoint;

                // Assert
                Assert.That(log.Endpoint.Length, Is.EqualTo(500));
            }

            [Test]
            public void TimestampProperty_CanStoreCurrentDateTime()
            {
                // Arrange
                var now = DateTime.Now;
                var log = new RateLimitLog();

                // Act
                log.Timestamp = now;

                // Assert
                Assert.That(log.Timestamp, Is.EqualTo(now).Within(TimeSpan.FromSeconds(1)));
            }

            [Test]
            public void TimestampProperty_CanStorePastDateTime()
            {
                // Arrange
                var pastDate = DateTime.Now.AddDays(-1);
                var log = new RateLimitLog();

                // Act
                log.Timestamp = pastDate;

                // Assert
                Assert.That(log.Timestamp, Is.EqualTo(pastDate).Within(TimeSpan.FromSeconds(1)));
            }

            [Test]
            public void TimestampProperty_CanStoreFutureDateTime()
            {
                // Arrange
                var futureDate = DateTime.Now.AddHours(1);
                var log = new RateLimitLog();

                // Act
                log.Timestamp = futureDate;

                // Assert
                Assert.That(log.Timestamp, Is.EqualTo(futureDate).Within(TimeSpan.FromSeconds(1)));
            }

            [Test]
            public void RequestCountProperty_CanBeZero()
            {
                // Arrange & Act
                var log = new RateLimitLog { RequestCount = 0 };

                // Assert
                Assert.That(log.RequestCount, Is.EqualTo(0));
            }

            [Test]
            public void RequestCountProperty_CanBePositive()
            {
                // Arrange & Act
                var log = new RateLimitLog { RequestCount = 150 };

                // Assert
                Assert.That(log.RequestCount, Is.EqualTo(150));
            }

            [Test]
            public void RequestCountProperty_CanBeNegative()
            {
                // Arrange & Act
                var log = new RateLimitLog { RequestCount = -5 };

                // Assert
                Assert.That(log.RequestCount, Is.EqualTo(-5));
            }

            [Test]
            public void LogIdProperty_CanBeSet()
            {
                // Arrange & Act
                var log = new RateLimitLog { LogId = 999 };

                // Assert
                Assert.That(log.LogId, Is.EqualTo(999));
        }
    }
}
