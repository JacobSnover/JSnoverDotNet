using jsnover.net.blazor.Models;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public enum FamilyPhotoDigestRunResult
    {
        BaselineEstablished,
        NoNewPhotos,
        Sent,
        AlreadyRunning
    }

    public sealed class FamilyPhotoDigestService
    {
        private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(15);
        private readonly jsnoverdotnetdbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly SmtpEmailOptions _smtpOptions;
        private readonly FamilyPhotoDigestOptions _digestOptions;

        public FamilyPhotoDigestService(
            jsnoverdotnetdbContext db,
            IEmailSender emailSender,
            IOptions<SmtpEmailOptions> smtpOptions,
            IOptions<FamilyPhotoDigestOptions> digestOptions)
        {
            _db = db;
            _emailSender = emailSender;
            _smtpOptions = smtpOptions.Value;
            _digestOptions = digestOptions.Value;
        }

        public async Task<FamilyPhotoDigestRunResult> RunAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var leaseId = Guid.NewGuid();
            var stateExists = await _db.FamilyPhotoDigestState
                .AnyAsync(state => state.Id == 1, cancellationToken);

            if (!stateExists)
                throw new InvalidOperationException("Apply the family photo digest migration before running the job.");

            var leaseAcquired = await _db.FamilyPhotoDigestState
                .Where(state => state.Id == 1 &&
                    (!state.LeaseExpiresAtUtc.HasValue || state.LeaseExpiresAtUtc <= now))
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(state => state.LeaseId, (Guid?)leaseId)
                    .SetProperty(state => state.LeaseExpiresAtUtc, (DateTime?)now.Add(LeaseDuration)), cancellationToken);

            if (leaseAcquired == 0)
                return FamilyPhotoDigestRunResult.AlreadyRunning;

            try
            {
                var state = await _db.FamilyPhotoDigestState.SingleAsync(state => state.Id == 1, cancellationToken);
                if (!state.BaselineEstablished)
                {
                    var existingPhotoIds = await _db.StandalonePhoto
                        .Where(photo => photo.IsPublished)
                        .Select(photo => photo.PhotoId)
                        .ToListAsync(cancellationToken);

                    _db.FamilyPhotoDigestDelivery.AddRange(existingPhotoIds.Select(photoId =>
                        new FamilyPhotoDigestDelivery
                        {
                            PhotoId = photoId,
                            DeliveredAtUtc = now,
                            WasBaseline = true
                        }));

                    state.BaselineEstablished = true;
                    state.BaselineEstablishedAtUtc = now;
                    await _db.SaveChangesAsync(cancellationToken);
                    return FamilyPhotoDigestRunResult.BaselineEstablished;
                }

                var newPhotos = await _db.StandalonePhoto
                    .Where(photo => photo.IsPublished &&
                        !_db.FamilyPhotoDigestDelivery.Any(delivery => delivery.PhotoId == photo.PhotoId))
                    .OrderBy(photo => photo.UploadDate)
                    .ThenBy(photo => photo.PhotoId)
                    .ToListAsync(cancellationToken);

                if (newPhotos.Count == 0)
                    return FamilyPhotoDigestRunResult.NoNewPhotos;

                var recipients = ParseRecipients(_digestOptions.Recipients);
                if (recipients.Count == 0)
                    throw new InvalidOperationException("Family photo digest recipients are not configured.");

                var message = CreateMessage(recipients);
                await _emailSender.SendAsync(message, cancellationToken);

                _db.FamilyPhotoDigestDelivery.AddRange(newPhotos.Select(photo =>
                    new FamilyPhotoDigestDelivery
                    {
                        PhotoId = photo.PhotoId,
                        DeliveredAtUtc = DateTime.UtcNow,
                        WasBaseline = false
                    }));
                await _db.SaveChangesAsync(cancellationToken);
                return FamilyPhotoDigestRunResult.Sent;
            }
            finally
            {
                await _db.FamilyPhotoDigestState
                    .Where(state => state.Id == 1 && state.LeaseId == leaseId)
                    .ExecuteUpdateAsync(updates => updates
                        .SetProperty(state => state.LeaseId, (Guid?)null)
                        .SetProperty(state => state.LeaseExpiresAtUtc, (DateTime?)null),
                        CancellationToken.None);
            }
        }

        private MimeMessage CreateMessage(IReadOnlyList<MailboxAddress> recipients)
        {
            var galleryUrl = GetSafePhotoUrl(_digestOptions.GalleryUrl);
            if (galleryUrl == null)
                throw new InvalidOperationException("Family photo gallery URL must be an absolute HTTP or HTTPS URL.");

            var encodedGalleryUrl = WebUtility.HtmlEncode(galleryUrl);
            var message = new MimeMessage
            {
                Subject = "New photos added to the family album"
            };
            message.From.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
            message.To.Add(new MailboxAddress(_smtpOptions.FromName, _smtpOptions.FromAddress));
            foreach (var recipient in recipients)
                message.Bcc.Add(recipient);

            message.Body = new BodyBuilder
            {
                TextBody = $"New photos have been added to the family album.\n\nVisit the album: {galleryUrl}",
                HtmlBody = $"<p>New photos have been added to the family album.</p><p><a href=\"{encodedGalleryUrl}\">View the family album</a></p>"
            }.ToMessageBody();
            return message;
        }

        private static List<MailboxAddress> ParseRecipients(string recipients)
        {
            return (recipients ?? string.Empty)
                .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(address => MailboxAddress.Parse(address.Trim()))
                .GroupBy(address => address.Address, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();
        }

        private static string GetSafePhotoUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? uri.AbsoluteUri
                : null;
        }
    }
}