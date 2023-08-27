using System.ComponentModel.DataAnnotations;

namespace jsnover.net.DataTransferObjects.BlogModels
{
    public class SubscribeModel
    {
        [Required]
        [EmailAddress]
        [StringLength(maximumLength: 250, ErrorMessage = "Email Address is too long.")]
        public string Email { get; set; }
    }
}
