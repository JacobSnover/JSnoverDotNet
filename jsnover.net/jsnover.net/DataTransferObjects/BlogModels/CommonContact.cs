using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.DataTransferObjects.BlogModels
{
    public class CommonContact
    {
        [StringLength(maximumLength: 50, ErrorMessage = "Must be under 50 characters")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(maximumLength: 256, ErrorMessage = "Email Address is too long.")]
        public string Email { get; set; }

        [StringLength(maximumLength: 500, ErrorMessage = "Please keep comment under 500 Characters")]
        public string Body { get; set; }

        public bool Liked { get; set; } = false;

        public bool Subscribe { get; set; }
    }
}
