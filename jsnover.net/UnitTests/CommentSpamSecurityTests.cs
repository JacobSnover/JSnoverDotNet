using NUnit.Framework;
using Moq;
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
    /// Security tests for spam detection in comments.
    /// Tests content validation, duplicate detection, and spam keyword filtering.
    /// </summary>
    [TestFixture]
    public class CommentSpamSecurityTests
    {
        private Mock<jsnoverdotnetdbContext> _mockDbContext;
        private CommentSpamService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<jsnoverdotnetdbContext>();
            _service = new CommentSpamService(_mockDbContext.Object);
        }

        #region Valid Comment Tests

        [Test]
        public async Task IsSpam_ValidComment_ReturnsFalse()
        {
            // Arrange
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("user@example.com", "Nice photo!");

            // Assert
            Assert.That(result, Is.False, "Valid short comment should pass");
        }

        [Test]
        public async Task IsSpam_LongerValidComment_ReturnsFalse()
        {
            // Arrange
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("user@example.com", "This is a wonderful photograph! The composition is excellent and the lighting is perfect.");

            // Assert
            Assert.That(result, Is.False, "Longer valid comment should pass");
        }

        #endregion

        #region Length Validation Tests

        [Test]
        public async Task IsSpam_MessageTooShort_ReturnsTrue()
        {
            // Arrange - Less than 5 characters
            // Act
            var result = await _service.IsSpam("user@example.com", "Hi!");

            // Assert
            Assert.That(result, Is.True, "Message less than 5 characters should be spam");
        }

        [Test]
        public async Task IsSpam_MessageExactlyFiveChars_ReturnsFalse()
        {
            // Arrange
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("user@example.com", "Hello");

            // Assert
            Assert.That(result, Is.False, "Message with exactly 5 characters should pass");
        }

        [Test]
        public async Task IsSpam_MessageTooLong_ReturnsTrue()
        {
            // Arrange - More than 1000 characters
            var longMessage = new string('a', 1001);

            // Act
            var result = await _service.IsSpam("user@example.com", longMessage);

            // Assert
            Assert.That(result, Is.True, "Message longer than 1000 characters should be spam");
        }

        [Test]
        public async Task IsSpam_MessageExactly1000Chars_ReturnsFalse()
        {
            // Arrange
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);
            var message1000 = new string('a', 1000);

            // Act
            var result = await _service.IsSpam("user@example.com", message1000);

            // Assert
            Assert.That(result, Is.False, "Message with exactly 1000 characters should pass");
        }

        #endregion

        #region HTML Tag Detection Tests

        [Test]
        public async Task IsSpam_ContainsScriptTag_ReturnsTrue()
        {
            // Arrange
            // Act
            var result = await _service.IsSpam("user@example.com", "<script>alert('xss')</script>");

            // Assert
            Assert.That(result, Is.True, "Comment with script tag should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_ContainsIframeTag_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Check this <iframe src='malicious.com'></iframe>");

            // Assert
            Assert.That(result, Is.True, "Comment with iframe tag should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_ContainsImgTag_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Look at <img src='x' onerror='alert(1)'>");

            // Assert
            Assert.That(result, Is.True, "Comment with img tag should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_ContainsAnyHtmlTag_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "This has a <div onclick='bad()'> tag");

            // Assert
            Assert.That(result, Is.True, "Any HTML tag should be flagged as spam");
        }

        #endregion

        #region Excessive URL Detection Tests

        [Test]
        public async Task IsSpam_ContainsTwoUrls_ReturnsFalse()
        {
            // Arrange
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("user@example.com", "Check http://example.com and https://another.com");

            // Assert
            Assert.That(result, Is.False, "Two links should be allowed");
        }

        [Test]
        public async Task IsSpam_ContainsThreeUrls_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Check http://a.com http://b.com http://c.com");

            // Assert
            Assert.That(result, Is.True, "Three or more links should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_ContainsWWWUrls_CountsCorrectly()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Visit www.site1.com and www.site2.com and www.site3.com");

            // Assert
            Assert.That(result, Is.True, "Three www URLs should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_ContainsFtpUrl_CountsCorrectly()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Download from ftp://files.com ftp://backup.com ftp://archive.com");

            // Assert
            Assert.That(result, Is.True, "Three FTP URLs should be flagged as spam");
        }

        #endregion

        #region Spam Keyword Detection Tests

        [Test]
        public async Task IsSpam_ContainsViagraKeyword_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Buy viagra now at discount prices");

            // Assert
            Assert.That(result, Is.True, "Comment with 'viagra' keyword should be flagged");
        }

        [Test]
        public async Task IsSpam_ContainsCasinoKeyword_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Try our best casino and win big");

            // Assert
            Assert.That(result, Is.True, "Comment with 'casino' keyword should be flagged");
        }

        [Test]
        public async Task IsSpam_ContainsBitcoinKeyword_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Invest in bitcoin cryptocurrency now");

            // Assert
            Assert.That(result, Is.True, "Comment with 'bitcoin' keyword should be flagged");
        }

        [Test]
        public async Task IsSpam_ContainsClickHereKeyword_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "Great opportunities! Click here to learn more");

            // Assert
            Assert.That(result, Is.True, "Comment with 'click here' keyword should be flagged");
        }

        [Test]
        public async Task IsSpam_CaseInsensitiveKeywordDetection_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "BUY VIAGRA NOW");

            // Assert
            Assert.That(result, Is.True, "Keyword detection should be case-insensitive");
        }

        [Test]
        public async Task IsSpam_NoSpamKeywords_ReturnsFalse()
        {
            // Arrange
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("user@example.com", "This photo is really beautiful and well composed");

            // Assert
            Assert.That(result, Is.False, "Comment without spam keywords should pass");
        }

        #endregion

        #region Profanity Detection Tests

        [Test]
        public async Task IsSpam_ContainsProfanity_ReturnsTrue()
        {
            // Arrange & Act
            var result = await _service.IsSpam("user@example.com", "This is f*** amazing");

            // Assert
            Assert.That(result, Is.True, "Comment with profanity should be flagged");
        }

        #endregion

        #region Duplicate Detection Tests

        [Test]
        public async Task IsSpam_SameEmailIdenticalMessage_ReturnsTrue()
        {
            // Arrange - Same email with identical message within 1 hour
            var email = "spammer@example.com";
            var message = "Check out our great deals!";
            
            var recentComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = email,
                Message = message,
                SubmitDate = DateTime.UtcNow.AddMinutes(-30)
            };

            var logs = new List<PhotoComment> { recentComment }.AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam(email, message);

            // Assert
            Assert.That(result, Is.True, "Identical message within 1 hour should be flagged as duplicate");
        }

        [Test]
        public async Task IsSpam_SimilarMessageOver80Percent_ReturnsTrue()
        {
            // Arrange - Messages with >80% similarity
            var email = "spammer@example.com";
            var originalMessage = "Nice photo! Really good!";
            var similarMessage = "Nice photo! Really goid!";  // Very similar (typo)

            var recentComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = email,
                Message = originalMessage,
                SubmitDate = DateTime.UtcNow.AddMinutes(-30)
            };

            var logs = new List<PhotoComment> { recentComment }.AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam(email, similarMessage);

            // Assert
            Assert.That(result, Is.True, "Message >80% similar within 1 hour should be flagged");
        }

        [Test]
        public async Task IsSpam_SameEmailOldMessageNotSpam_ReturnsFalse()
        {
            // Arrange - Same email but message from >1 hour ago
            var email = "user@example.com";
            var message = "Nice photo!";
            
            var oldComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = email,
                Message = message,
                SubmitDate = DateTime.UtcNow.AddHours(-2)  // 2 hours ago
            };

            var logs = new List<PhotoComment> { oldComment }.AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam(email, "Another nice photo!");

            // Assert
            Assert.That(result, Is.False, "Message from >1 hour ago should not trigger duplicate detection");
        }

        [Test]
        public async Task IsSpam_NoRecentCommentsFromEmail_ReturnsFalse()
        {
            // Arrange - New email with no history
            var logs = new List<PhotoComment>().AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("newuser@example.com", "Great work!");

            // Assert
            Assert.That(result, Is.False, "New email with no history should not be flagged");
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public async Task IsSpam_NullEmail_ReturnsTrue()
        {
            // Act
            var result = await _service.IsSpam(null, "Nice photo");

            // Assert
            Assert.That(result, Is.True, "Null email should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_EmptyEmail_ReturnsTrue()
        {
            // Act
            var result = await _service.IsSpam("", "Nice photo");

            // Assert
            Assert.That(result, Is.True, "Empty email should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_NullMessage_ReturnsTrue()
        {
            // Act
            var result = await _service.IsSpam("user@example.com", null);

            // Assert
            Assert.That(result, Is.True, "Null message should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_EmptyMessage_ReturnsTrue()
        {
            // Act
            var result = await _service.IsSpam("user@example.com", "");

            // Assert
            Assert.That(result, Is.True, "Empty message should be flagged as spam");
        }

        [Test]
        public async Task IsSpam_WhitespaceMessage_ReturnsTrue()
        {
            // Act
            var result = await _service.IsSpam("user@example.com", "   ");

            // Assert
            Assert.That(result, Is.True, "Whitespace-only message should be flagged as spam");
        }

        #endregion

        #region Database Error Handling Tests

        [Test]
        public async Task IsSpam_DatabaseException_ReturnsFalse()
        {
            // Arrange - Simulate database error during duplicate check
            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.Where(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>())
                .Returns(new AsyncQueryable<PhotoComment>(new List<PhotoComment>().AsQueryable()))
            ;
            mockDbSet.Setup(x => x
                .Where(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>())
                .Select(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, string>>>())
            ).Throws(new Exception("Database error"));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam("user@example.com", "This is a test");

            // Assert
            Assert.That(result, Is.False, "Database errors should fail open (not flag as spam)");
        }

        #endregion

        #region Levenshtein Distance Tests

        [Test]
        public async Task IsSpam_IdenticalStrings_CalculatesSimilarityAsOne()
        {
            // Arrange - Identical strings should have 100% similarity
            var email = "user@example.com";
            var message = "This is a test message";

            var recentComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = email,
                Message = message,
                SubmitDate = DateTime.UtcNow.AddMinutes(-30)
            };

            var logs = new List<PhotoComment> { recentComment }.AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam(email, message);

            // Assert
            Assert.That(result, Is.True, "Identical messages should be flagged as duplicate");
        }

        [Test]
        public async Task IsSpam_DifferentStrings_CalculatesSimilarityCorrectly()
        {
            // Arrange - Very different strings should have low similarity
            var email = "user@example.com";
            var originalMessage = "AAAAAAAAAA";
            var differentMessage = "BBBBBBBBBB";

            var recentComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = email,
                Message = originalMessage,
                SubmitDate = DateTime.UtcNow.AddMinutes(-30)
            };

            var logs = new List<PhotoComment> { recentComment }.AsQueryable();
            var mockDbSet = CreateMockDbSet(logs);
            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.IsSpam(email, differentMessage);

            // Assert
            Assert.That(result, Is.False, "Very different messages should not be flagged");
        }

        #endregion

        #region Helper Methods

        private Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockDbSet = new Mock<DbSet<T>>();
            mockDbSet.As<IAsyncEnumerable<T>>()
                .Setup(x => x.GetAsyncEnumerator(default))
                .Returns(new AsyncEnumerator<T>(data.GetEnumerator()));
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.Provider)
                .Returns(data.Provider);
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.Expression)
                .Returns(data.Expression);
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.ElementType)
                .Returns(data.ElementType);
            mockDbSet.As<IQueryable<T>>()
                .Setup(x => x.GetEnumerator())
                .Returns(data.GetEnumerator());

            return mockDbSet;
        }

        public class AsyncEnumerator<T> : System.Collections.Generic.IAsyncEnumerator<T>
        {
            private readonly System.Collections.Generic.IEnumerator<T> _inner;

            public AsyncEnumerator(System.Collections.Generic.IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current
            {
                get { return _inner.Current; }
            }

            public async ValueTask<bool> MoveNextAsync()
            {
                return _inner.MoveNext();
            }

            public async ValueTask DisposeAsync()
            {
                _inner?.Dispose();
            }
        }

        // Helper for async queries in mocks
        internal class AsyncQueryable<T> : IAsyncEnumerable<T>
        {
            private readonly IEnumerable<T> _enumerable;

            public AsyncQueryable(IEnumerable<T> enumerable) 
            {
                _enumerable = enumerable;
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(System.Threading.CancellationToken cancellationToken = default)
            {
                return new AsyncEnumerator<T>(_enumerable.GetEnumerator());
            }
        }

        #endregion
    }
}
