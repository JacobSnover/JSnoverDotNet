using jsnover.net.blazor.Constants;
using jsnover.net.blazor.DataTransferObjects.Interfaces;
using jsnover.net.blazor.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.blazor.DataTransferObjects.BlogModels
{
    public class BlogEditModel : IDtoMapper<BlogEditModel, Blog[]>
    {
        [Required] public Blog Blog { get; set; } = new Blog();
        [Required] public List<Tag> Tags { get; set; }
        [Required] public List<Photos> Photos { get; set; }

        public async static Task<BlogEditModel> MapToBlogPageModel(int id)
        {
            var model = new BlogEditModel();
            using (var db = new jsnoverdotnetdbContext())
            {
                var DBBlogList = await db.Blog.ToArrayAsync();
                var tempBlog = DBBlogList.SingleOrDefault(blog => blog.Id == id);
                var DBPhotoList = await db.Photos.ToArrayAsync();
                var DBTagList = await db.Tag.ToArrayAsync();
                model.Blog.Id = id;
                model.Blog.SubmitDate = tempBlog.SubmitDate;
                model.Blog.Title = tempBlog.Title;
                model.Blog.Topic = tempBlog.Topic;
                model.Blog.Body = tempBlog.Body;
                model.Tags = DBTagList.Where(tag => tag.BlogId == id).ToList();
                model.Photos = DBPhotoList.Where(photo => photo.BlogId == id).ToList();
                model.Blog.Likes = tempBlog.Likes;
                model.Blog.Views = tempBlog.Views;
                model.Blog.Published = tempBlog.Published;
            }
            return model;
        }

        public static Blog[] MapToDto(BlogEditModel blog)
        {
            var blogArray = new Blog[7];

            blog.Blog.EditDate = DateTime.Now;

            blogArray[BlogIndexes.Blog] = blog.Blog;

            blog.Photos.ForEach(photo => photo.Blog = null);// set blog to null to prevent entity tracking
            blogArray[BlogIndexes.Photos] = new Blog
            {
                Photos = blog.Photos.Where(photo => photo.Id != 0 && photo.Link != Keywords.Remove).ToList()// id zero means photo already existed
            };
            blogArray[BlogIndexes.NewPhotos] = new Blog
            {
                Photos = blog.Photos.Where(photo => photo.Id == 0 && photo.Link != Keywords.Remove).ToList()
            };
            blogArray[BlogIndexes.RemovePhotos] = new Blog
            {
                Photos = blog.Photos.Where(photo => photo.Link == Keywords.Remove).ToList()
            };

            blog.Tags.ForEach(tag => tag.Blog = null);// set blog to null to prevent entity tracking
            blogArray[BlogIndexes.Tags] = new Blog
            {
                Tag = blog.Tags.Where(tag => tag.Id != 0 && tag.Name != Keywords.Remove).ToList()// id zero means tag already existed
            };
            blogArray[BlogIndexes.NewTags] = new Blog
            {
                Tag = blog.Tags.Where(tag => tag.Id == 0 && tag.Name != Keywords.Remove).ToList()
            };
            blogArray[BlogIndexes.RemoveTags] = new Blog
            {
                Tag = blog.Tags.Where(tag => tag.Name == Keywords.Remove).ToList()
            };

            return blogArray;
        }
    }
}
