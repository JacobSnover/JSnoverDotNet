using jsnover.net.blazor.Constants;
using jsnover.net.blazor.DataTransferObjects.Interfaces;
using jsnover.net.blazor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace jsnover.net.blazor.DataTransferObjects.BlogModels
{
    public class NewBlogModel : IDtoMapper<NewBlogModel, Blog[]>
    {
        public int BlogId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Body { get; set; }
        public DateTime SubmitDate { get; set; }
        public DateTime? EditDate { get; set; }
        [Required]
        public string Topic { get; set; }
        [Required]
        public string Tags { get; set; }
        public bool? Published { get; set; }

        public List<string> Photos { get; set; } = new List<string>();

        public static Blog[] MapToDto(NewBlogModel newBlog)
        {
            // hold in 3 different blogs in this array, this is due to EFCore tracking the blogs when uploaded
            // by keeping them as 3 separate blogs, I can upload everything in one repo function
            var blogs = new Blog[3];

            blogs[BlogIndexes.Blog] = new Blog
            {
                Title = newBlog.Title,
                Topic = newBlog.Topic,
                Body = newBlog.Body,
                SubmitDate = SetSubmitDate(),
                Published = newBlog.Published
            };

            blogs[BlogIndexes.Photos] = new Blog
            {
                Photos = MapPhotos(newBlog.Photos)
            };

            blogs[BlogIndexes.Tags] = new Blog
            {
                Tag = CleanTags(newBlog.Tags)
            };

            return blogs;
        }

        private static ICollection<Photos> MapPhotos(List<string> photos)
        {
            var modelPhotos = new HashSet<Photos>();
            for (int i = 0; i < photos.Count; i++)
            {
                modelPhotos.Add(new Photos
                {
                    Id = 0,
                    Link = photos[i].Trim(),
                    BlogId = 0
                });
            }
            return modelPhotos;
        }

        private static ICollection<Tag> CleanTags(string tags)
        {
            var tagSet = new HashSet<Tag>();
            var cleanTags = tags.Split(',');
            for (int i = 0; i < cleanTags.Length; i++)
            {
                if (cleanTags[i] != string.Empty)
                {
                    tagSet.Add(new Tag
                    {
                        Id = 0,
                        Name = cleanTags[i].Trim(),
                        BlogId = 0
                    });
                }
            }
            return tagSet;
        }

        private static DateTime SetSubmitDate()
        {
            return DateTime.Now;
        }
    }
}