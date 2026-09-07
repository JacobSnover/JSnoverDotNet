using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class FamilyChoreService
    {
        private static readonly HashSet<string> ChoreBoardAllowedEmails = new(StringComparer.OrdinalIgnoreCase)
        {
            "snoverjacob@yahoo.com",
            "twoyellowducks@yahoo.com",
            "actonlauren@yahoo.com"
        };

        private readonly jsnoverdotnetdbContext _db;

        public FamilyChoreService(jsnoverdotnetdbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<List<Chore>> GetChoresAsync()
        {
            return await _db.Chore
                .OrderBy(chore => chore.Status)
                .ThenBy(chore => chore.Name)
                .ToListAsync();
        }

        public async Task<Subscribers> GetAuthorizedSubscriberAsync(string email)
        {
            var normalizedEmail = email?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedEmail) || !ChoreBoardAllowedEmails.Contains(normalizedEmail))
            {
                return null;
            }

            return await _db.Subscribers
                .Where(subscriber => subscriber.Email == normalizedEmail && subscriber.IsApproved)
                .OrderByDescending(subscriber => subscriber.UserName != null)
                .FirstOrDefaultAsync();
        }

        public async Task AddChoreAsync(Chore chore, Subscribers subscriber)
        {
            ArgumentNullException.ThrowIfNull(chore);
            ArgumentNullException.ThrowIfNull(subscriber);

            chore.Name = chore.Name?.Trim();
            chore.Worker = string.Empty;
            chore.Notes = chore.Notes?.Trim();
            chore.Status = false;
            chore.LastCompleted = null;
            chore.Worker = string.Empty;

            _db.Chore.Add(chore);
            await _db.SaveChangesAsync();
        }

        private static string GetWorkerName(Subscribers subscriber)
        {
            if (!string.IsNullOrWhiteSpace(subscriber.UserName))
            {
                return subscriber.UserName.Trim();
            }

            return subscriber.Email.Split('@')[0].Trim();
        }

        public async Task UpdateChoreAsync(Chore chore)
        {
            ArgumentNullException.ThrowIfNull(chore);

            var existing = await _db.Chore.FirstOrDefaultAsync(item => item.Id == chore.Id);
            if (existing == null)
            {
                return;
            }

            existing.Name = chore.Name?.Trim();
            existing.Notes = chore.Notes?.Trim();

            await _db.SaveChangesAsync();
        }

        public async Task ToggleCompleteAsync(int choreId, Subscribers subscriber)
        {
            ArgumentNullException.ThrowIfNull(subscriber);

            var chore = await _db.Chore.FirstOrDefaultAsync(item => item.Id == choreId);
            if (chore == null)
            {
                return;
            }

            chore.Status = !chore.Status;
            if (chore.Status)
            {
                chore.LastCompleted = DateTime.UtcNow;
                chore.Worker = GetWorkerName(subscriber);
            }
            else
            {
                chore.LastCompleted = null;
                chore.Worker = string.Empty;
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteChoreAsync(int choreId)
        {
            var chore = await _db.Chore.FirstOrDefaultAsync(item => item.Id == choreId);
            if (chore == null)
            {
                return;
            }

            _db.Chore.Remove(chore);
            await _db.SaveChangesAsync();
        }
    }
}