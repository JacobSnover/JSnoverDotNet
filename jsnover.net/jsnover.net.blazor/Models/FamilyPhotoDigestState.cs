using System;

namespace jsnover.net.blazor.Models
{
    public class FamilyPhotoDigestState
    {
        public int Id { get; set; }
        public bool BaselineEstablished { get; set; }
        public DateTime? BaselineEstablishedAtUtc { get; set; }
        public Guid? LeaseId { get; set; }
        public DateTime? LeaseExpiresAtUtc { get; set; }
    }
}