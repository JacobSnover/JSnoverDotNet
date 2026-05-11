using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class HealthTrackerService
    {
        private readonly jsnoverdotnetdbContext _context;

        public HealthTrackerService(jsnoverdotnetdbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all health entries
        /// </summary>
        public async Task<List<HealthEntry>> GetEntriesAsync()
        {
            return await _context.HealthEntry
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Get health entries within a date range
        /// </summary>
        public async Task<List<HealthEntry>> GetEntriesAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.HealthEntry
                .Where(e => e.Date >= startDate && e.Date <= endDate)
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Get aggregated monthly data
        /// </summary>
        public async Task<List<MonthlyHealthData>> GetMonthlyDataAsync()
        {
            var entries = await _context.HealthEntry
                .ToListAsync();

            var monthlyData = entries
                .GroupBy(e => new { e.Date.Year, e.Date.Month })
                .Select(g => new MonthlyHealthData
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    AvgSystolic = Math.Round(g.Average(e => e.Systolic), 1),
                    AvgDiastolic = Math.Round(g.Average(e => e.Diastolic), 1),
                    AvgHeartRate = Math.Round(g.Average(e => e.HeartRate), 1),
                    EntryCount = g.Count()
                })
                .OrderByDescending(m => m.Year)
                .ThenByDescending(m => m.Month)
                .ToList();

            return monthlyData;
        }

        /// <summary>
        /// Get all-time aggregated data
        /// </summary>
        public async Task<AllTimeHealthData> GetAllTimeDataAsync()
        {
            var entries = await _context.HealthEntry
                .ToListAsync();

            if (!entries.Any())
            {
                return new AllTimeHealthData();
            }

            return new AllTimeHealthData
            {
                TotalEntries = entries.Count,
                AvgSystolic = Math.Round(entries.Average(e => e.Systolic), 1),
                AvgDiastolic = Math.Round(entries.Average(e => e.Diastolic), 1),
                AvgHeartRate = Math.Round(entries.Average(e => e.HeartRate), 1),
                MaxSystolic = entries.Max(e => e.Systolic),
                MinSystolic = entries.Min(e => e.Systolic),
                MaxDiastolic = entries.Max(e => e.Diastolic),
                MinDiastolic = entries.Min(e => e.Diastolic),
                MaxHeartRate = entries.Max(e => e.HeartRate),
                MinHeartRate = entries.Min(e => e.HeartRate),
                MaxPounds = entries.Where(e => e.Notes.Contains("lbs")).Any() ? entries.Where(e => e.Notes.Contains("lbs")).Max(e => int.Parse(e.Notes.Replace("lbs", "").Split(' ')[0])) : 0,
                MinPounds = entries.Where(e => e.Notes.Contains("lbs")).Any() ? entries.Where(e => e.Notes.Contains("lbs")).Min(e => int.Parse(e.Notes.Replace("lbs", "").Split(' ')[0])) : 0,
                MaxKilograms = entries.Where(e => e.Notes.Contains("kg")).Any() ? entries.Where(e => e.Notes.Contains("kg")).Max(e => double.Parse(e.Notes.Replace("kg", "").Split(' ')[1])) : 0,
                MinKilograms = entries.Where(e => e.Notes.Contains("kg")).Any() ? entries.Where(e => e.Notes.Contains("kg")).Min(e => double.Parse(e.Notes.Replace("kg", "").Split(' ')[1])) : 0
            };
        }

        /// <summary>
        /// Add a new health entry
        /// </summary>
        public async Task<HealthEntry> AddEntryAsync(HealthEntry entry)
        {
            _context.HealthEntry.Add(entry);
            await _context.SaveChangesAsync();

            return entry;
        }

        /// <summary>
        /// Update an existing health entry
        /// </summary>
        public async Task<HealthEntry> UpdateEntryAsync(HealthEntry entry)
        {
            var existingEntry = await _context.HealthEntry.FindAsync(entry.Id);
            if (existingEntry == null)
            {
                throw new InvalidOperationException($"Health entry with ID {entry.Id} not found.");
            }

            existingEntry.Date = entry.Date;
            existingEntry.Systolic = entry.Systolic;
            existingEntry.Diastolic = entry.Diastolic;
            existingEntry.HeartRate = entry.HeartRate;
            existingEntry.Notes = entry.Notes;

            _context.HealthEntry.Update(existingEntry);
            await _context.SaveChangesAsync();

            return existingEntry;
        }

        /// <summary>
        /// Delete a health entry by ID
        /// </summary>
        public async Task DeleteEntryAsync(int id)
        {
            var entry = await _context.HealthEntry.FindAsync(id);
            if (entry == null)
            {
                throw new InvalidOperationException($"Health entry with ID {id} not found.");
            }

            _context.HealthEntry.Remove(entry);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Export health entries
        /// </summary>
        public async Task<List<HealthEntry>> ExportAsync()
        {
            return await _context.HealthEntry
                .OrderByDescending(e => e.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Import health entries
        /// </summary>
        public async Task ImportAsync(List<HealthEntry> entries)
        {
            foreach (var entry in entries)
            {
                _context.HealthEntry.Add(entry);
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Data class for monthly aggregated health data
    /// </summary>
    public class MonthlyHealthData
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public double AvgSystolic { get; set; }
        public double AvgDiastolic { get; set; }
        public double AvgHeartRate { get; set; }
        public int EntryCount { get; set; }
    }

    /// <summary>
    /// Data class for all-time aggregated health data
    /// </summary>
    public class AllTimeHealthData
    {
        public int TotalEntries { get; set; }
        public double AvgSystolic { get; set; }
        public double AvgDiastolic { get; set; }
        public double AvgHeartRate { get; set; }
        public int MaxSystolic { get; set; }
        public int MinSystolic { get; set; }
        public int MaxDiastolic { get; set; }
        public int MinDiastolic { get; set; }
        public int MaxHeartRate { get; set; }
        public int MinHeartRate { get; set; }
        public int MaxPounds { get; set; }
        public int MinPounds { get; set; }
        public double MaxKilograms { get; set; }
        public double MinKilograms { get; set; }
    }
}
