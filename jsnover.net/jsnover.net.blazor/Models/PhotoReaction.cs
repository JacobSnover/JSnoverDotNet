using System;

namespace jsnover.net.blazor.Models
{
    public class PhotoReaction
    {
        public int ReactionId { get; set; }
        public int PhotoId { get; set; }
        public string ReactionType { get; set; } // Emoji type: 👍, ❤️, 😂, 😮, 😢
        public string SessionId { get; set; } // Tracks per-session reactions for guests
        public string UserId { get; set; } // Nullable: for authenticated users (links to AspNetUsers.Id)
        public DateTime CreatedDate { get; set; }

        public virtual StandalonePhoto Photo { get; set; }
    }
}
