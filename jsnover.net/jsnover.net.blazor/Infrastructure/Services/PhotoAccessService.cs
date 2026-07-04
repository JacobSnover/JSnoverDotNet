using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using jsnover.net.blazor.Models;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class PhotoAccessService
    {
        private readonly jsnoverdotnetdbContext _db;

        public PhotoAccessService(jsnoverdotnetdbContext db)
        {
            _db = db;
        }

        public async Task<bool> IsEmailApprovedAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var normalized = email.Trim();
            // Check for approved subscriber
            var approved = await _db.Subscribers.AnyAsync(s => s.Email == normalized && s.IsApproved);
            return approved;
        }

        public async Task<bool> IsEmailRegisteredAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var normalized = email.Trim();
            return await _db.Subscribers.AnyAsync(s => s.Email == normalized);
        }

        public async Task<Subscribers> GetOrCreateSubscriberByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return new Subscribers();

            var normalized = email.Trim();
            var existing = await _db.Subscribers.FirstOrDefaultAsync(s => s.Email == normalized);

            if (existing != null) return existing;

            // Return new in-memory instance
            return new Subscribers { Email = normalized };
        }

        public async Task CreateAccessRequestAsync(Subscribers subscriber)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            if (string.IsNullOrWhiteSpace(subscriber.Email)) 
                throw new ArgumentException("Email is required", nameof(subscriber));

            var normalized = subscriber.Email.Trim();

            // Check for duplicate email
            var exists = await _db.Subscribers.AnyAsync(s => s.Email == normalized);
            if (exists)
                throw new InvalidOperationException("This email is already registered.");

            var now = DateTime.UtcNow;
            subscriber.Email = normalized;
            subscriber.UserName = subscriber.UserName?.Trim();
            subscriber.SubmittedDate = now;
            subscriber.SubscribeDate = now;
            subscriber.IsApproved = false;
            subscriber.AwaitingApproval = true;

            _db.Subscribers.Add(subscriber);
            await _db.SaveChangesAsync();
        }

        public async Task<Subscribers> GetSubscriberStatusByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var normalized = email.Trim();
            return await _db.Subscribers.FirstOrDefaultAsync(s => s.Email == normalized);
        }
    }
}
