using jsnover.net.blazor.DataTransferObjects.Common;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class EmailService
    {        
        internal static async Task NotifySnover(ContactModel contactRequest)
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("jsnover@jsnover.net", "jsnover.net"),
                Subject = "jsnover.net New Contact Request",
                PlainTextContent =
                $"NEW CONTACT REQUEST - Name: {contactRequest.Name}, Email: {contactRequest.Email}, Request Body: {contactRequest.Body}, ISSUE WITH SITE: {contactRequest.Issue}",
                HtmlContent =
                $"<strong>NEW CONTACT REQUEST</strong><br/>" +
                $"<strong>Name</strong>: {contactRequest.Name}<br/>" +
                $"<strong>Email</strong>: {contactRequest.Email}<br/>" +
                $"<strong>Request Body</strong>: {contactRequest.Body}<br/>" +
                $"<strong>ISSUE WITH SITE</strong>: {contactRequest.Issue}<br/>"
            };
            msg.AddTo(new EmailAddress("snoverjacob@yahoo.com", "Randy"));
            await SendEmail(msg);
        }

        private static async Task SendEmail(SendGridMessage msg)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var apiKey = Environment.GetEnvironmentVariable("SendGrid:ApiKey");

            if (apiKey is null)
            {
                apiKey = builder.GetValue<string>("SendGrid:ApiKey");
            }

            var client = new SendGridClient(apiKey);
            
            await client.SendEmailAsync(msg);
        }
    }
}