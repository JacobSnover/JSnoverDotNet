using System;
using System.Collections.Generic;

namespace jsnover.net.blazor.Models
{
    public partial class Commentors
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Body { get; set; }
        public int BlogId { get; set; }
        public DateTime DatePosted { get; set; }
        public string UserName { get; set; }
        public bool Liked { get; set; }
        public bool Subscribe { get; set; }

        public virtual Blog Blog { get; set; }
    }
}
