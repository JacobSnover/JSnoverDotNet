using System;

namespace jsnover.net.blazor.Models
{
    public class RateLimitLog
    {
        public int LogId { get; set; }
        public string IpAddress { get; set; }
        public string Endpoint { get; set; }
        public DateTime Timestamp { get; set; }
        public int RequestCount { get; set; }
    }
}
