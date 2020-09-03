using jsnover.net.blazor.DataTransferObjects.Common;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using System;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class EmailService
    {        
        internal static async Task NotifySnover(ContactModel contactRequest)
        {
            var messageToSend = new MimeMessage
            {
                Sender = new MailboxAddress("jsnover.net", "fourseasonflora@gmail.com"),
                Subject = "jsnover.net New Contact Request",
            };

            messageToSend.Body = new TextPart(TextFormat.Html)
            {
                Text =
                $"<strong>NEW CONTACT REQUEST</strong><br/>" +
                $"<strong>Name</strong>: {contactRequest.Name}" +
                $"<strong>Email</strong>: {contactRequest.Email}<br/>" +
                $"<strong>Request Body</strong>: {contactRequest.Body}" +
                $"<strong>ISSUE WITH SITE</strong>: {contactRequest.Issue}<br/>"
            };

            messageToSend.To.Add(new MailboxAddress("Randy", "snoverjacob@yahoo.com"));

            await SendEmail(messageToSend);
        }

        private static async Task SendEmail(MimeMessage messageToSend)
        {
            using (var smtp = new MailKit.Net.Smtp.SmtpClient())
            {
                smtp.MessageSent += (sender, args) => { };
                smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;

                var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

                var email = Environment.GetEnvironmentVariable("Flora:Email", EnvironmentVariableTarget.Process);
                var password = Environment.GetEnvironmentVariable("Flora:Password", EnvironmentVariableTarget.Process);

                if (email is null)
                {
                    email = builder.GetValue<string>("Flora:Email");
                }
                if (password is null)
                {
                    password = builder.GetValue<string>("Flora:Password");
                }


                await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.Auto);
                await smtp.AuthenticateAsync(email, password);
                await smtp.SendAsync(messageToSend);
                await smtp.DisconnectAsync(true);
            }
        }
    }
}