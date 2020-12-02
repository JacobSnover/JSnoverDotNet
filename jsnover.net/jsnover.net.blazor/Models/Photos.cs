using System;
using System.Collections.Generic;

namespace jsnover.net.blazor.Models
{
    public partial class Photos
    {
        public int Id { get; set; }
        public string Link { get; set; }
        public int BlogId { get; set; }

        public virtual Blog Blog { get; set; }
    }
}
