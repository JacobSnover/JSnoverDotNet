using jsnover.net.DataTransferObjects.Interfaces;
using jsnover.net.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.DataTransferObjects.Common
{
    public class ContactModel : IDtoMapper<ContactModel, ContactRequest>
    {
        [StringLength(maximumLength: 50, ErrorMessage = "Must be under 50 characters")]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(maximumLength: 256, ErrorMessage = "Email Address is too long.")]
        public string Email { get; set; }

        [StringLength(maximumLength: 500, ErrorMessage = "Please keep comment under 500 Characters")]
        public string Body { get; set; }

        [StringLength(maximumLength: 50, ErrorMessage = "Must be under 100 characters")]
        public string CompanyName { get; set; }

        public bool Issue { get; set; } = false;

        public bool Business { get; set; } = false;

        public static ContactRequest MapToDto(ContactModel comment)
        {
            return new ContactRequest
            {
                Id = 0,
                DatePosted = DateTime.Now.Date,
                Email = comment.Email,
                UserName = comment.Name ?? null,
                Body = comment.Body ?? string.Empty,
                CompanyName = comment.CompanyName,
                Issue = comment.Issue,
                Business = comment.Business
            };
        }
    }
}
