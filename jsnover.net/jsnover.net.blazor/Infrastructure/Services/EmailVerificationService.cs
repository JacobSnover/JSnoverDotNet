using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace jsnover.net.blazor.Infrastructure.Services
{
    /// <summary>
    /// Service for managing email verification of photo comments.
    /// Generates verification codes, validates them, and sends verification emails.
    /// </summary>
    public class EmailVerificationService
    {
        private readonly jsnoverdotnetdbContext _db;
        private const int VerificationCodeExpiryMinutes = 24 * 60; // 24 hours
        private const string FromEmail = "jsnover@jsnover.net";
        private const string FromName = "jsnover.net";
        private const string AdminEmail = "snoverjacob@yahoo.com";

        public EmailVerificationService(jsnoverdotnetdbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        /// <summary>
        /// Generates a random 8-character verification code.
        /// </summary>
        /// <returns>Random alphanumeric verification code</returns>
        public string GenerateVerificationCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Range(0, 8)
                .Select(_ => chars[random.Next(chars.Length)])
                .ToArray());
        }

        /// <summary>
        /// Verifies if the provided code matches the code stored for the email address.
        /// </summary>
        /// <param name="email">Email address to verify</param>
        /// <param name="code">Verification code provided by user</param>
        /// <returns>True if code is valid and not expired; false otherwise</returns>
        public async Task<bool> VerifyCode(string email, string code)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                    return false;

                var photoComment = await _db.PhotoComment
                    .FirstOrDefaultAsync(pc =>
                        pc.Email.ToLower() == email.ToLower() &&
                        pc.VerificationCode == code &&
                        pc.VerificationCodeExpiry.HasValue &&
                        pc.VerificationCodeExpiry > DateTime.UtcNow);

                if (photoComment == null)
                    return false;

                // Mark comment as verified
                photoComment.IsVerified = true;
                photoComment.VerificationCode = null;
                photoComment.VerificationCodeExpiry = null;
                
                _db.PhotoComment.Update(photoComment);
                await _db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends a verification email with a link to verify the comment.
        /// </summary>
        /// <param name="email">Recipient email address</param>
        /// <param name="name">Name of the commenter</param>
        /// <param name="verificationCode">Verification code to include in email</param>
        /// <returns>True if email sent successfully; false otherwise</returns>
        public async Task<bool> SendVerificationEmail(string email, string name, string verificationCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(verificationCode))
                    return false;

                string verificationLink = $"https://jsnover.net/verify-comment?code={verificationCode}";

                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(FromEmail, FromName),
                    Subject = "Verify Your Photo Comment - jsnover.net",
                    PlainTextContent =
                        $"Hi {name},\n\n" +
                        $"Thank you for commenting on jsnover.net! Please verify your comment by clicking the link below:\n\n" +
                        $"{verificationLink}\n\n" +
                        $"This link will expire in 24 hours.\n\n" +
                        $"If you did not submit this comment, please ignore this email.\n\n" +
                        $"Best regards,\nThe jsnover.net Team",
                    HtmlContent =
                        $"<p>Hi {name},</p>" +
                        $"<p>Thank you for commenting on jsnover.net! Please verify your comment by clicking the link below:</p>" +
                        $"<p><a href=\"{verificationLink}\" style=\"background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block;\">Verify Comment</a></p>" +
                        $"<p>This link will expire in 24 hours.</p>" +
                        $"<p>If you did not submit this comment, please ignore this email.</p>" +
                        $"<p>Best regards,<br/>The jsnover.net Team</p>"
                };

                msg.AddTo(new EmailAddress(email, name));

                // Send email via SendGrid
                return await SendEmailAsync(msg);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sends email via SendGrid API.
        /// </summary>
        private async Task<bool> SendEmailAsync(SendGridMessage msg)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SendGrid:ApiKey");

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    // Try reading from appsettings as fallback
                    var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: false)
                        .Build();
                    apiKey = builder.GetValue<string>("SendGrid:ApiKey");
                }

                if (string.IsNullOrWhiteSpace(apiKey))
                    return false;

                var client = new SendGrid.SendGridClient(apiKey);
                var response = await client.SendEmailAsync(msg);

                return response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                       response.StatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
