using MimeKit;
using System.Threading;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public interface IEmailSender
    {
        Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default);
    }
}