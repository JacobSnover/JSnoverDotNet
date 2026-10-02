using MailKit.Security;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public sealed class SmtpEmailOptions
    {
        public const string SectionName = "Smtp";

        public string Host { get; set; } = "mail5008.site4now.net";
        public int Port { get; set; } = 587;
        public SecureSocketOptions SecurityOptions { get; set; } = SecureSocketOptions.StartTls;
        public string Username { get; set; } = "postmaster@jsnover.net";
        public string Password { get; set; } = string.Empty;
        public string FromAddress { get; set; } = "postmaster@jsnover.net";
        public string FromName { get; set; } = "jsnover.net";
    }
}