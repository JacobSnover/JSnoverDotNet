using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service for detecting and filtering spam comments on photos.
    /// Checks for duplicate submissions, suspicious content, and profanity.
    /// </summary>
    public class CommentSpamService
    {
        private readonly jsnoverdotnetdbContext _db;
        private const int DuplicateCheckWindowMinutes = 60; // Check for duplicates within 1 hour
        private const double SimilarityThreshold = 0.80; // 80% match = spam
        private const int MaxLinksInComment = 2;
        private const int MinCommentLength = 5;
        private const int MaxCommentLength = 1000;

        // List of common spam keywords and phrases
        private static readonly HashSet<string> SpamKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "viagra", "cialis", "casino", "poker", "lottery", "bitcoin", "cryptocurrency",
            "forex", "weight loss", "diet pills", "erectile dysfunction", "click here",
            "buy now", "limited offer", "act now", "xxx", "adult", "porn", "sex",
            "cheap watches", "replica", "counterfeit", "fake rolex", "nigerian prince",
            "work from home", "make money fast", "guaranteed income", "easy money",
            "enlargement", "enhancement", "prescription", "pharm", "pharmacy",
            "payday loan", "debt consolidation", "credit repair", "bad credit"
        };

        public CommentSpamService(jsnoverdotnetdbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Determines if a comment is spam based on multiple checks.
        /// </summary>
        /// <param name="email">Commenter email</param>
        /// <param name="message">Comment message</param>
        /// <returns>True if comment is detected as spam; false otherwise</returns>
        public async Task<bool> IsSpam(string email, string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(message))
                    return true;

                // Check 1: Message length
                if (message.Length < MinCommentLength || message.Length > MaxCommentLength)
                    return true;

                // Check 2: HTML tags
                if (ContainsHtmlTags(message))
                    return true;

                // Check 3: Excessive links
                if (CountUrls(message) > MaxLinksInComment)
                    return true;

                // Check 4: Suspicious keywords
                if (ContainsSpamKeywords(message))
                    return true;

                // Check 5: Duplicate submission (same email + similar message)
                if (await IsDuplicateSubmission(email, message))
                    return true;

                // Check 6: Profanity/offensive content
                if (ContainsOfensiveContent(message))
                    return true;

                return false;
            }
            catch (Exception)
            {
                // On error, assume not spam (fail open)
                return false;
            }
        }

        /// <summary>
        /// Checks if message contains HTML tags.
        /// </summary>
        private bool ContainsHtmlTags(string message)
        {
            // Look for common HTML tags
            return Regex.IsMatch(message, @"<[^>]+>", RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// Counts the number of URLs in the message.
        /// </summary>
        private int CountUrls(string message)
        {
            // Simple URL detection: http://, https://, www., or ftp://
            var urlPattern = @"(https?://|www\.|ftp://)";
            return Regex.Matches(message, urlPattern, RegexOptions.IgnoreCase).Count;
        }

        /// <summary>
        /// Checks if message contains known spam keywords.
        /// </summary>
        private bool ContainsSpamKeywords(string message)
        {
            var lowerMessage = message.ToLower();

            foreach (var keyword in SpamKeywords)
            {
                // Use word boundary to match whole words
                if (Regex.IsMatch(lowerMessage, $@"\b{Regex.Escape(keyword)}\b"))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if this is a duplicate submission (same email with similar message).
        /// </summary>
        private async Task<bool> IsDuplicateSubmission(string email, string message)
        {
            try
            {
                var timeWindow = DateTime.UtcNow.AddMinutes(-DuplicateCheckWindowMinutes);

                // Find recent comments from the same email
                var recentComments = await _db.PhotoComment
                    .Where(pc =>
                        pc.Email.ToLower() == email.ToLower() &&
                        pc.SubmitDate > timeWindow)
                    .Select(pc => pc.Message)
                    .ToListAsync();

                if (!recentComments.Any())
                    return false;

                // Check if any recent comment is similar (>80% match)
                foreach (var recentMessage in recentComments)
                {
                    double similarity = CalculateSimilarity(message, recentMessage);
                    if (similarity >= SimilarityThreshold)
                        return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Calculates string similarity using Levenshtein distance.
        /// Returns a value between 0 and 1 (1 = identical, 0 = completely different).
        /// </summary>
        private double CalculateSimilarity(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
                return 0;

            int distance = LevenshteinDistance(str1, str2);
            int maxLength = Math.Max(str1.Length, str2.Length);

            return 1.0 - (double)distance / maxLength;
        }

        /// <summary>
        /// Calculates Levenshtein distance between two strings.
        /// </summary>
        private int LevenshteinDistance(string str1, string str2)
        {
            int[,] dp = new int[str1.Length + 1, str2.Length + 1];

            for (int i = 0; i <= str1.Length; i++)
                dp[i, 0] = i;

            for (int j = 0; j <= str2.Length; j++)
                dp[0, j] = j;

            for (int i = 1; i <= str1.Length; i++)
            {
                for (int j = 1; j <= str2.Length; j++)
                {
                    int cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost);
                }
            }

            return dp[str1.Length, str2.Length];
        }

        /// <summary>
        /// Checks for offensive/profane content.
        /// </summary>
        private bool ContainsOfensiveContent(string message)
        {
            // Basic profanity filter - can be extended
            var profanityPatterns = new[]
            {
                @"\bf[*u]ck", @"\bass", @"\bbitch", @"\bdamn", @"\bhell",
                @"\bsh[*i]t", @"\bcrap", @"\bwhore", @"\bslut", @"\bd[*a]mn"
            };

            var lowerMessage = message.ToLower();
            foreach (var pattern in profanityPatterns)
            {
                if (Regex.IsMatch(lowerMessage, pattern, RegexOptions.IgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
