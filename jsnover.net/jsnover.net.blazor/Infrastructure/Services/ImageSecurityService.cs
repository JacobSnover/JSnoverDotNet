using System;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service providing image security utilities including robot meta tags and URL validation.
    /// This service uses static methods and can be used with or without dependency injection.
    /// </summary>
    public class ImageSecurityService
    {
        /// <summary>
        /// Generates HTML meta tags to prevent search engine indexing (robots noindex).
        /// </summary>
        /// <returns>HTML meta tag string for robots noindex directive</returns>
        public static string GenerateRobotsMetaTags()
        {
            try
            {
                return "<meta name=\"robots\" content=\"noindex, follow\">";
            }
            catch (Exception)
            {
                return "<meta name=\"robots\" content=\"noindex, follow\">";
            }
        }

        /// <summary>
        /// Validates an image URL for basic security requirements.
        /// </summary>
        /// <param name="url">The URL to validate</param>
        /// <returns>True if the URL passes validation; false otherwise</returns>
        public static bool ValidateImageUrl(string url)
        {
            try
            {
                // Null or empty check
                if (string.IsNullOrWhiteSpace(url))
                    return false;

                // Basic length validation
                if (url.Length > 2048)
                    return false;

                // Must start with https://
                if (!url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return false;

                // Additional validation: URL must be well-formed
                if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
                    return false;

                // Validate that the scheme is https
                if (uriResult.Scheme != Uri.UriSchemeHttps)
                    return false;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
