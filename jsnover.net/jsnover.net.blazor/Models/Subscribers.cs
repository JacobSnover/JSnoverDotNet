using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jsnover.net.blazor.Models
{
    public partial class Subscribers
    {
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public DateTime SubscribeDate { get; set; }
        public string UserName { get; set; }
        public DateTime SubmittedDate { get; set; }
        public bool IsApproved { get; set; }
        public bool AwaitingApproval { get; set; }
    }
}
