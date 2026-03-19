using System;
using System.Collections.Generic;

namespace jsnover.net.blazor.Models
{
    public class StandalonePhoto
    {
        public StandalonePhoto()
        {
            Comments = new HashSet<PhotoComment>();
            Reactions = new HashSet<PhotoReaction>();
        }

        public int PhotoId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public int DisplayOrder { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsPublished { get; set; }
        public string Tags { get; set; } // Comma-separated tags

        public virtual ICollection<PhotoComment> Comments { get; set; }
        public virtual ICollection<PhotoReaction> Reactions { get; set; }
    }
}
