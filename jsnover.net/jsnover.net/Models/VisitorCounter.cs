using System;
using System.Collections.Generic;

namespace jsnover.net.Models
{
    public partial class VisitorCounter
    {
        public int Id { get; set; }
        public int? VisitorCount { get; set; }
        public int? VisitorCountHundreds { get; set; }
        public DateTime? VisitorDate { get; set; }
    }
}
