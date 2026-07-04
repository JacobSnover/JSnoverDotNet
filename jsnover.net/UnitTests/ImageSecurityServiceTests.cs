using NUnit.Framework;
using System;
using jsnover.net.blazor.Infrastructure.Services;

namespace UnitTests
{
    /// <summary>
    /// Unit tests for ImageSecurityService.
    /// Tests image URL validation and robots meta tag generation.
    /// </summary>
    [TestFixture]
    public class ImageSecurityServiceTests
    {
        #region ValidateImageUrl Tests

        [Test]
        public void ValidateImageUrl_ValidHttpsUrl_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com/photo.jpg";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateImageUrl_HttpUrl_ReturnsFalse()
        {
            // Arrange
            string url = "http://example.com/photo.jpg";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_UrlLongerThan2048_ReturnsFalse()
        {
            // Arrange
            string url = "https://example.com/" + new string('a', 2100);

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_UrlExactly2048_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com/" + new string('a', 2028);

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateImageUrl_InvalidUri_ReturnsFalse()
        {
            // Arrange
            string url = "not a valid url";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_NullUrl_ReturnsFalse()
        {
            // Arrange
            string url = null;

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_EmptyUrl_ReturnsFalse()
        {
            // Arrange
            string url = "";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_WhitespaceUrl_ReturnsFalse()
        {
            // Arrange
            string url = "   ";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_FtpScheme_ReturnsFalse()
        {
            // Arrange
            string url = "ftp://example.com/photo.jpg";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void ValidateImageUrl_ComplexHttpsUrl_ReturnsTrue()
        {
            // Arrange
            string url = "https://cdn.example.com:443/images/photo.jpg?size=large&format=png";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateImageUrl_HttpsWithSubdomain_ReturnsTrue()
        {
            // Arrange
            string url = "https://images.example.co.uk/gallery/photo.jpg";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion

        #region GenerateRobotsMetaTags Tests

        [Test]
        public void GenerateRobotsMetaTags_ReturnsMetaTag()
        {
            // Arrange & Act
            var result = ImageSecurityService.GenerateRobotsMetaTags();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Not.Empty);
        }

        [Test]
        public void GenerateRobotsMetaTags_ContainsCorrectContent()
        {
            // Arrange & Act
            var result = ImageSecurityService.GenerateRobotsMetaTags();

            // Assert
            Assert.That(result, Does.Contain("<meta name=\"robots\" content=\"noindex, follow\">"));
        }

        [Test]
        public void GenerateRobotsMetaTags_IsValidHtmlMetaTag()
        {
            // Arrange & Act
            var result = ImageSecurityService.GenerateRobotsMetaTags();

            // Assert
            Assert.That(result, Does.StartWith("<meta "));
            Assert.That(result, Does.EndWith(">"));
            Assert.That(result, Does.Contain("name=\"robots\""));
            Assert.That(result, Does.Contain("content="));
        }

        [Test]
        public void GenerateRobotsMetaTags_ContainsNoindex()
        {
            // Arrange & Act
            var result = ImageSecurityService.GenerateRobotsMetaTags();

            // Assert
            Assert.That(result, Does.Contain("noindex"));
        }

        [Test]
        public void GenerateRobotsMetaTags_ContainsFollow()
        {
            // Arrange & Act
            var result = ImageSecurityService.GenerateRobotsMetaTags();

            // Assert
            Assert.That(result, Does.Contain("follow"));
        }

        [Test]
        public void GenerateRobotsMetaTags_MultipleCallsReturnSame()
        {
            // Arrange & Act
            var result1 = ImageSecurityService.GenerateRobotsMetaTags();
            var result2 = ImageSecurityService.GenerateRobotsMetaTags();
            var result3 = ImageSecurityService.GenerateRobotsMetaTags();

            // Assert
            Assert.That(result1, Is.EqualTo(result2));
            Assert.That(result2, Is.EqualTo(result3));
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        public void ValidateImageUrl_CaseSensitivity_HttpsCapital_ReturnsTrue()
        {
            // Arrange
            string url = "HTTPS://EXAMPLE.COM/photo.jpg";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void ValidateImageUrl_SpecialCharactersInPath_ReturnsTrue()
        {
            // Arrange
            string url = "https://example.com/photos/2024-01-15_vacation.jpg";

            // Act
            var result = ImageSecurityService.ValidateImageUrl(url);

            // Assert
            Assert.That(result, Is.True);
        }

        #endregion
    }
}
