using System;
using System.Collections.Generic;

namespace jsnover.net.Models
{
    public partial class Blog
    {
        public Blog()
        {
            Commentors = new HashSet<Commentors>();
            Photos = new HashSet<Photos>();
            Tag = new HashSet<Tag>();
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime SubmitDate { get; set; }
        public DateTime? EditDate { get; set; }
        public string Topic { get; set; }
        public int? Likes { get; set; }
        public int? Views { get; set; }
        public bool? Published { get; set; }

        public virtual ICollection<Commentors> Commentors { get; set; }
        public virtual ICollection<Photos> Photos { get; set; }
        public virtual ICollection<Tag> Tag { get; set; }
    }
}
