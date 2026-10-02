using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpEmailOptions _options;

        public SmtpEmailSender(IOptions<SmtpEmailOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new InvalidOperationException("SMTP host is not configured.");

            if (string.IsNullOrWhiteSpace(_options.Username) || string.IsNullOrWhiteSpace(_options.Password))
                throw new InvalidOperationException("SMTP authentication credentials are not configured.");

            if (_options.SecurityOptions != SecureSocketOptions.StartTls &&
                _options.SecurityOptions != SecureSocketOptions.SslOnConnect)
            {
                throw new InvalidOperationException("SMTP must use STARTTLS or implicit TLS.");
            }

            using var client = new SmtpClient();
            await client.ConnectAsync(_options.Host, _options.Port, _options.SecurityOptions, cancellationToken);
            await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
    }
}