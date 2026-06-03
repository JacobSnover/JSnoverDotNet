namespace jsnover.net.blazor.DataTransferObjects.HealthData
{
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
}
