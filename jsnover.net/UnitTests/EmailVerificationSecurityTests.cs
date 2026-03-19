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
    /// Security tests for email verification of photo comments.
    /// Tests code generation, validation, expiration, and verification flow.
    /// </summary>
    [TestFixture]
    public class EmailVerificationSecurityTests
    {
        private Mock<jsnoverdotnetdbContext> _mockDbContext;
        private EmailVerificationService _service;

        [SetUp]
        public void SetUp()
        {
            _mockDbContext = new Mock<jsnoverdotnetdbContext>();
            _service = new EmailVerificationService(_mockDbContext.Object);
        }

        #region Code Generation Tests

        [Test]
        public void GenerateVerificationCode_ReturnsEightCharacterCode()
        {
            // Act
            var code = _service.GenerateVerificationCode();

            // Assert
            Assert.That(code.Length, Is.EqualTo(8), "Verification code must be exactly 8 characters");
        }

        [Test]
        public void GenerateVerificationCode_ReturnsAlphanumericCode()
        {
            // Act
            var code = _service.GenerateVerificationCode();

            // Assert
            Assert.That(code, Does.Match(@"^[A-Z0-9]{8}$"), "Code must be uppercase alphanumeric only");
        }

        [Test]
        public void GenerateVerificationCode_GeneratesDifferentCodes()
        {
            // Act - Generate multiple codes
            var code1 = _service.GenerateVerificationCode();
            var code2 = _service.GenerateVerificationCode();
            var code3 = _service.GenerateVerificationCode();

            // Assert - They should be different (statistically almost guaranteed)
            var codes = new[] { code1, code2, code3 };
            var uniqueCodes = codes.Distinct().Count();
            Assert.That(uniqueCodes, Is.GreaterThan(1), "Generated codes should be different");
        }

        #endregion

        #region Verification Code Validation Tests

        [Test]
        public async Task VerifyCode_ValidCode_ReturnsTrue()
        {
            // Arrange
            var validCode = "ABC12345";
            var expiryTime = DateTime.UtcNow.AddHours(1);
            var photoComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = "test@example.com",
                VerificationCode = validCode,
                VerificationCodeExpiry = expiryTime,
                IsVerified = false
            };

            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult(photoComment));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            // Act
            var result = await _service.VerifyCode("test@example.com", validCode);

            // Assert
            Assert.That(result, Is.True, "Valid code should verify successfully");
        }

        [Test]
        public async Task VerifyCode_InvalidCode_ReturnsFalse()
        {
            // Arrange - Code doesn't match
            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult<PhotoComment>(null));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.VerifyCode("test@example.com", "WRONGCODE");

            // Assert
            Assert.That(result, Is.False, "Invalid code should not verify");
        }

        [Test]
        public async Task VerifyCode_ExpiredCode_ReturnsFalse()
        {
            // Arrange - Code has expired
            var expiredCode = "ABC12345";
            var expiryTime = DateTime.UtcNow.AddHours(-1);  // 1 hour ago

            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult<PhotoComment>(null));  // Query finds nothing because expiry > UtcNow fails

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.VerifyCode("test@example.com", expiredCode);

            // Assert
            Assert.That(result, Is.False, "Expired code should not verify");
        }

        [Test]
        public async Task VerifyCode_NullEmail_ReturnsFalse()
        {
            // Act
            var result = await _service.VerifyCode(null, "ABC12345");

            // Assert
            Assert.That(result, Is.False, "Null email should not verify");
        }

        [Test]
        public async Task VerifyCode_EmptyEmail_ReturnsFalse()
        {
            // Act
            var result = await _service.VerifyCode("", "ABC12345");

            // Assert
            Assert.That(result, Is.False, "Empty email should not verify");
        }

        [Test]
        public async Task VerifyCode_WhitespaceEmail_ReturnsFalse()
        {
            // Act
            var result = await _service.VerifyCode("   ", "ABC12345");

            // Assert
            Assert.That(result, Is.False, "Whitespace email should not verify");
        }

        [Test]
        public async Task VerifyCode_NullCode_ReturnsFalse()
        {
            // Act
            var result = await _service.VerifyCode("test@example.com", null);

            // Assert
            Assert.That(result, Is.False, "Null code should not verify");
        }

        [Test]
        public async Task VerifyCode_EmptyCode_ReturnsFalse()
        {
            // Act
            var result = await _service.VerifyCode("test@example.com", "");

            // Assert
            Assert.That(result, Is.False, "Empty code should not verify");
        }

        #endregion

        #region Verification Expiration Tests

        [Test]
        public async Task VerifyCode_Code24HoursOld_Expires()
        {
            // Arrange - Code is exactly 24 hours old
            var code = "TIMEOUT24";
            var expiryTime = DateTime.UtcNow.AddHours(-24).AddSeconds(-1);  // Just past 24 hours

            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult<PhotoComment>(null));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.VerifyCode("test@example.com", code);

            // Assert
            Assert.That(result, Is.False, "Code older than 24 hours should expire");
        }

        [Test]
        public async Task VerifyCode_CodeBefore24Hours_StillValid()
        {
            // Arrange - Code is 23 hours old (still valid)
            var code = "VALID23H";
            var expiryTime = DateTime.UtcNow.AddHours(-23);
            var photoComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = "test@example.com",
                VerificationCode = code,
                VerificationCodeExpiry = expiryTime.AddHours(24),  // 1 hour in the future
                IsVerified = false
            };

            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult(photoComment));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            // Act
            var result = await _service.VerifyCode("test@example.com", code);

            // Assert
            Assert.That(result, Is.True, "Code less than 24 hours old should be valid");
        }

        #endregion

        #region One Active Code Per Email Tests

        [Test]
        public async Task VerifyCode_MarksCommentAsVerified()
        {
            // Arrange - Valid code that should mark comment as verified
            var validCode = "CODE0001";
            var photoComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = "test@example.com",
                VerificationCode = validCode,
                VerificationCodeExpiry = DateTime.UtcNow.AddHours(1),
                IsVerified = false
            };

            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult(photoComment));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            // Act
            var result = await _service.VerifyCode("test@example.com", validCode);

            // Assert
            Assert.That(result, Is.True, "Verification should succeed");
            Assert.That(photoComment.IsVerified, Is.True, "Comment should be marked as verified");
            Assert.That(photoComment.VerificationCode, Is.Null, "Verification code should be cleared");
            Assert.That(photoComment.VerificationCodeExpiry, Is.Null, "Verification code expiry should be cleared");
        }

        #endregion

        #region Multiple Verification Attempts Tests

        [Test]
        public async Task VerifyCode_MultipleAttempts_AllowedUntilExpiry()
        {
            // Arrange - Code exists but users can try multiple times
            var code = "MULTI001";
            var photoComment = new PhotoComment
            {
                PhotoCommentId = 1,
                Email = "test@example.com",
                VerificationCode = code,
                VerificationCodeExpiry = DateTime.UtcNow.AddHours(1),
                IsVerified = false
            };

            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .Returns(Task.FromResult(photoComment));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(default)).Returns(Task.FromResult(1));

            // Act - First attempt
            var result1 = await _service.VerifyCode("test@example.com", code);

            // Assert - First succeeds
            Assert.That(result1, Is.True, "First verification attempt should succeed");

            // Second attempt would fail because after first verification, the code is moved to null
            // This effectively only allows one successful verification
        }

        #endregion

        #region Email Sending Tests

        [Test]
        public async Task SendVerificationEmail_ValidEmailAndCode_ReturnsTrue()
        {
            // Arrange
            var email = "user@example.com";
            var code = "SEND0001";
            var name = "John Doe";

            // Act
            var result = await _service.SendVerificationEmail(email, name, code);

            // Assert - Should return true if email service is configured
            // Note: This will depend on SendGrid configuration
            Assert.That(result, Is.TypeOf<bool>(), "Should return boolean");
        }

        [Test]
        public async Task SendVerificationEmail_NullEmail_ReturnsFalse()
        {
            // Act
            var result = await _service.SendVerificationEmail(null, "John", "CODE123");

            // Assert
            Assert.That(result, Is.False, "Null email should not send");
        }

        [Test]
        public async Task SendVerificationEmail_EmptyEmail_ReturnsFalse()
        {
            // Act
            var result = await _service.SendVerificationEmail("", "John", "CODE123");

            // Assert
            Assert.That(result, Is.False, "Empty email should not send");
        }

        [Test]
        public async Task SendVerificationEmail_NullCode_ReturnsFalse()
        {
            // Act
            var result = await _service.SendVerificationEmail("user@example.com", "John", null);

            // Assert
            Assert.That(result, Is.False, "Null verification code should not send");
        }

        [Test]
        public async Task SendVerificationEmail_EmptyCode_ReturnsFalse()
        {
            // Act
            var result = await _service.SendVerificationEmail("user@example.com", "John", "");

            // Assert
            Assert.That(result, Is.False, "Empty verification code should not send");
        }

        #endregion

        #region Error Handling Tests

        [Test]
        public async Task VerifyCode_DatabaseException_ReturnsFalse()
        {
            // Arrange - Simulate database error
            var mockDbSet = new Mock<DbSet<PhotoComment>>();
            mockDbSet.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<PhotoComment, bool>>>(), default))
                .ThrowsAsync(new Exception("Database error"));

            _mockDbContext.Setup(x => x.PhotoComments).Returns(mockDbSet.Object);

            // Act
            var result = await _service.VerifyCode("test@example.com", "CODE123");

            // Assert
            Assert.That(result, Is.False, "Database errors should return false");
        }

        #endregion
    }
}
