using System;
using System.Collections.Generic;

namespace jsnover.net.blazor.Models
{
    public partial class ContactRequest
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Body { get; set; }
        public DateTime DatePosted { get; set; }
        public string CompanyName { get; set; }
        public bool Issue { get; set; }
        public bool Business { get; set; }
        public string UserName { get; set; }
    }
}
