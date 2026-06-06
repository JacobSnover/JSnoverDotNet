using System;

namespace jsnover.net.blazor.Models
{
    public partial class HealthEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int Systolic { get; set; } // 50-250
        public int Diastolic { get; set; } // 30-200
        public int HeartRate { get; set; } // 20-240
        public string Notes { get; set; } // Optional
        public string UserId { get; set; }
    }
}
