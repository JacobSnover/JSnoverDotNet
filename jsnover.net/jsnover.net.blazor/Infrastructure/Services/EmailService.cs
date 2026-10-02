using jsnover.net.blazor.DataTransferObjects.BlogModels;
using jsnover.net.blazor.DataTransferObjects.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net;
using System;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class EmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpEmailOptions _smtpOptions;

        public EmailService(
            IEmailSender emailSender,
            ILogger<EmailService> logger,
            IOptions<SmtpEmailOptions> smtpOptions)
        {
            _emailSender = emailSender;
            _logger = logger;
            _smtpOptions = smtpOptions.Value;
        }

        /// <summary>
        /// Notify Snover about new comment on blog
        /// </summary>
        /// <param name="comment"></param>
        /// <param name="blog"></param>
        internal async Task NotifySnover(BlogCommentModel comment, BlogDisplayModel blog)
        {
            var message = CreateMessage("New Contact Request jsnover.net");
            var bodyBuilder = new BodyBuilder
            {
                TextBody =
               $"New Comment on Blog: {blog.Title}, Name: {comment.Name}, Email: {comment.Email}, " +
               $"Subscribe: {comment.Subscribe}, Liked: {comment.Liked}, Comment: {comment.Body}",
                HtmlBody =
               $"<strong>New Comment on Blog</strong>: {Encode(blog.Title)}<br/>" +
                $"<strong>Name</strong>: {Encode(comment.Name)}<br/>" +
                $"<strong>Email</strong>: {Encode(comment.Email)}<br/>" +
                $"<strong>Subscribe</strong>: {comment.Subscribe}<br/>" +
                $"<strong>Liked</strong>: {comment.Liked}<br/>" +
                $"<strong>Comment</strong>: {Encode(comment.Body)}"
            };
            message.Body = bodyBuilder.ToMessageBody();
            await SendEmailAsync(message, "blog comment");
        }

        internal async Task NotifySnover(string subscriberEmail)
        {
            var message = CreateMessage("New Subscriber jsnover.net");
            message.Body = new BodyBuilder
            {
                TextBody = $"New Subscriber: {subscriberEmail}",
                HtmlBody = $"<strong>New Subscriber</strong>: {Encode(subscriberEmail)}<br/>"
            }.ToMessageBody();
            await SendEmailAsync(message, "subscriber");
        }

        internal async Task NotifySnover(ContactModel contactRequest)
        {
            var message = CreateMessage("jsnover.net New Contact Request");
            message.Body = new BodyBuilder
            {
                TextBody =
                $"NEW CONTACT REQUEST - Name: {contactRequest.Name}, Email: {contactRequest.Email}, Request Body: {contactRequest.Body}, ISSUE WITH SITE: {contactRequest.Issue}",
                HtmlBody =
                $"<strong>NEW CONTACT REQUEST</strong><br/>" +
                $"<strong>Name</strong>: {Encode(contactRequest.Name)}<br/>" +
                $"<strong>Email</strong>: {Encode(contactRequest.Email)}<br/>" +
                $"<strong>Request Body</strong>: {Encode(contactRequest.Body)}<br/>" +
                $"<strong>ISSUE WITH SITE</strong>: {contactRequest.Issue}<br/>"
            }.ToMessageBody();
            await SendEmailAsync(message, "contact request");
        }

        private MimeMessage CreateMessage(string subject)
        {
            var message = new MimeMessage
            {
                Subject = subject
            };
            message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
            message.To.Add(new MailboxAddress("Randy", "snoverjacob@yahoo.com"));
            return message;
        }

        private async Task SendEmailAsync(MimeMessage message, string notificationType)
        {
            try
            {
                await _emailSender.SendAsync(message);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to send {NotificationType} email notification.", notificationType);
            }
        }

        private static string Encode(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }
    }
}