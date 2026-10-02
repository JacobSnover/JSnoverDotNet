using System;

namespace jsnover.net.blazor.Models
{
    public class FamilyPhotoDigestDelivery
    {
        public int PhotoId { get; set; }
        public DateTime DeliveredAtUtc { get; set; }
        public bool WasBaseline { get; set; }
    }
}