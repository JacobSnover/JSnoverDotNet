namespace jsnover.net.blazor.DataTransferObjects.HealthData
{
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
