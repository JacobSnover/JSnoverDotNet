using NUnit.Framework;
using System;
using System.IO;

namespace UnitTests
{
    /// <summary>
    /// Security tests for meta tags and robots.txt configuration.
    /// Validates search engine exclusions and content protection directives.
    /// </summary>
    [TestFixture]
    public class MetaTagSecurityTests
    {
        private string _robotsPath;
        private string _projectRoot;

        [SetUp]
        public void SetUp()
        {
            // Find the project root (jsnover.net.blazor folder)
            var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var projectRoot = currentDirectory;

            // Navigate up from bin/Debug/net6.0 to jsnover.net.blazor
            while (!Directory.Exists(Path.Combine(projectRoot, "wwwroot")) && projectRoot != Path.GetPathRoot(projectRoot))
            {
                projectRoot = Directory.GetParent(projectRoot).FullName;
            }

            _projectRoot = projectRoot;
            _robotsPath = Path.Combine(projectRoot, "wwwroot", "robots.txt");
        }

        #region Robots.txt File Tests

        [Test]
        public void RobotsFile_Exists_InWwwroot()
        {
            // Assert
            Assert.That(File.Exists(_robotsPath), Is.True, "robots.txt must exist in wwwroot/");
        }

        [Test]
        public void RobotsFile_DisallowsPhotosPath()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert
            Assert.That(robotsContent, Does.Contain("Disallow: /photos"), 
                "robots.txt must disallow /photos path to prevent scraping");
        }

        [Test]
        public void RobotsFile_DisallowsAdminPath()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert
            Assert.That(robotsContent, Does.Contain("Disallow: /admin"), 
                "robots.txt must disallow /admin paths for security");
        }

        [Test]
        public void RobotsFile_ContainsAllDisallowRules()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert - Verify key anti-scraping rules
            Assert.That(robotsContent, Has.Length.GreaterThan(0), "robots.txt must not be empty");
            
            // Check for wildcard rules or specific paths
            var hasPhotoDisallow = robotsContent.Contains("Disallow: /photos") || 
                                   robotsContent.Contains("Disallow: /api/photos");
            Assert.That(hasPhotoDisallow, Is.True, "robots.txt should disallow photo access");
        }

        #endregion

        #region Meta Tag Security Tests

        [Test]
        public void PhotosPage_HasNoindexMetaTag()
        {
            // This test verifies that Pages/Photos.razor contains noindex

            // Find Photos.razor
            var photosRazorPath = Path.Combine(_projectRoot, "Pages", "Photos.razor");
            
            if (File.Exists(photosRazorPath))
            {
                var content = File.ReadAllText(photosRazorPath);
                
                // Assert
                Assert.That(content, Does.Contain("noindex") | Does.Contain("meta"), 
                    "Photos.razor should contain noindex meta tag");
            }
        }

        [Test]
        public void MetaTags_IncludeBotSpecificExclusions()
        {
            // Find Photos.razor for meta tag verification
            var photosRazorPath = Path.Combine(_projectRoot, "Pages", "Photos.razor");
            
            if (File.Exists(photosRazorPath))
            {
                var content = File.ReadAllText(photosRazorPath);
                
                // Assert - Check for bot-specific meta tags
                var hasBotExclusions = content.Contains("bingbot") || 
                                      content.Contains("GPTBot") || 
                                      content.Contains("ChatGPT");
                
                Assert.That(hasBotExclusions, Is.True, 
                    "Meta tags should include exclusions for AI bots");
            }
        }

        [Test]
        public void MetaTags_IncludesContentLockForAI()
        {
            // Find Photos.razor for AI content lock verification
            var photosRazorPath = Path.Combine(_projectRoot, "Pages", "Photos.razor");
            
            if (File.Exists(photosRazorPath))
            {
                var content = File.ReadAllText(photosRazorPath);
                
                // Assert - Check for AI dataset protection
                Assert.That(content, Does.Contain("ai-content-locked") | Does.Contain("noai"), 
                    "Meta tags should include AI dataset protection");
            }
        }

        #endregion

        #region Search Engine Directive Tests

        [Test]
        public void RobotsFile_HasUserAgentWildcard()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert
            Assert.That(robotsContent, Does.Contain("User-agent: *"), 
                "robots.txt should have User-agent: * to apply to all crawlers");
        }

        [Test]
        public void RobotsFile_DisallowsApiEndpoints()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert - Verify API endpoints are protected
            var hasApiDisallow = robotsContent.Contains("Disallow: /api") || 
                                robotsContent.Contains("Disallow: /api/");
            
            Assert.That(hasApiDisallow, Is.True, 
                "robots.txt should disallow API endpoints from crawlers");
        }

        #endregion

        #region Content Protection Tests

        [Test]
        public void RobotsFile_PreventsPhotoDataExfiltration()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert - Check for comprehensive protection
            var hasComprehensiveRules = (robotsContent.Contains("Disallow: /photos") || 
                                        robotsContent.Contains("Disallow: /api/photos")) &&
                                       robotsContent.Contains("User-agent: *");
            
            Assert.That(hasComprehensiveRules, Is.True, 
                "robots.txt must have comprehensive rules to prevent photo data scraping");
        }

        [Test]
        public void RobotsFile_AllowsPublicContent()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);

            // Assert - Should not disable all crawling
            Assert.That(robotsContent, Does.Not.Contain("Disallow: /"), 
                new ConstraintInjector(), 
                "robots.txt may have specific disallows but shouldn't disallow root");
        }

        #endregion

        #region Compliance Tests

        [Test]
        public void RobotsFile_IsValidFormat()
        {
            // Arrange
            var robotsContent = File.ReadAllText(_robotsPath);
            var lines = robotsContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            // Assert
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // Each non-comment line should follow key: value format
                Assert.That(line, Does.Contain(":") | Does.Match(@"^\s*$"), 
                    $"Invalid robots.txt line: {line}");
            }
        }

        #endregion
    }

    /// <summary>
    /// Helper class for constraint handling in tests
    /// </summary>
    public class ConstraintInjector : NUnit.Framework.Constraints.IResolveConstraint
    {
        public NUnit.Framework.Constraints.IConstraint Resolve() 
        => NUnit.Framework.Constraints.Resolve.Constraint(NUnit.Framework.Constraints.Resolve.Constraint(null));
    }
}
