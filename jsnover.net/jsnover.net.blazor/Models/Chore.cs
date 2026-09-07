using System;
using System.ComponentModel.DataAnnotations;

namespace jsnover.net.blazor.Models
{
    public partial class Chore
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public bool Status { get; set; } = false;

        public DateTime? LastCompleted { get; set; }

        [StringLength(100)]
        public string Worker { get; set; }

        [StringLength(2000)]
        public string Notes { get; set; }
    }
}