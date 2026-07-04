using System;

namespace jsnover.net.blazor.Models
{
    public class PhotoComment
    {
        public int CommentId { get; set; }
        public int? PhotoId { get; set; } // Nullable: link to StandalonePhoto (if provided)
        public int? BlogId { get; set; } // Nullable: link to Blog photo (if provided)
        public string Email { get; set; }
        public string Name { get; set; }
        public string Message { get; set; }
        public DateTime SubmitDate { get; set; }
        public bool IsVerified { get; set; }
        public bool IsApproved { get; set; }
        public string VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
        public string UserId { get; set; } // Nullable: for authenticated users (links to AspNetUsers.Id)

        public virtual StandalonePhoto Photo { get; set; }
        public virtual Blog Blog { get; set; }
    }
}
