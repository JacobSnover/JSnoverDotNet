using jsnover.net.blazor.Infrastructure.SqlRepo;
using jsnover.net.blazor.DataTransferObjects.BlogModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jsnover.net.blazor.Models;
using System.Text.RegularExpressions;

namespace jsnover.net.blazor.Infrastructure.Services
{
    public class BlogService
    {
        private static jsnoverdotnetdbContext db;

        private static async Task<List<BlogDisplayModel>> GetDataFromAzureSql()
        {
            db = new jsnoverdotnetdbContext();
            var blogList = new List<BlogDisplayModel>();
            var DBBlogList = await db.Blog.ToArrayAsync();
            var DBPhotoList = await db.Photos.ToArrayAsync();
            var DBTagList = await db.Tag.ToArrayAsync();
            var DBCommentorList = await db.Commentors.ToArrayAsync();

            for (int i = 0; i < DBBlogList.Length; i++)
            {
                blogList.Add(new BlogDisplayModel()
                {
                    BlogId = DBBlogList[i].Id,
                    Body = DBBlogList[i].Body,
                    Title = DBBlogList[i].Title,
                    Topic = DBBlogList[i].Topic,
                    SubmitDate = DBBlogList[i].SubmitDate.Date,
                    EditDate = DBBlogList[i].EditDate,
                    Tags = DBTagList.Where(tag => tag.BlogId == DBBlogList[i].Id).ToList(),
                    Photos = DBPhotoList.Where(photo => photo.BlogId == DBBlogList[i].Id).ToList(),
                    Commentors = DBCommentorList.Where(commentor => commentor.BlogId == DBBlogList[i].Id).ToList(),
                    Likes = DBBlogList[i].Likes,
                    Views = DBBlogList[i].Views,
                    Published = DBBlogList[i].Published
                });
            }
            return blogList;
        }

        public async Task CountVisitor()
        {
            try
            {
                db = new jsnoverdotnetdbContext();
                var tempList = db.VisitorCounter.ToArray();
                var tempCount = tempList.FirstOrDefault();
                tempCount.VisitorCount = tempCount?.VisitorCount + 1;
                db.VisitorCounter.Update(tempCount);
                await db.SaveChangesAsync();

                if (tempCount.VisitorCount % 100 == 0)
                {
                    var hundoCount = tempList.LastOrDefault();
                    db.VisitorCounter.Add(new VisitorCounter
                    {
                        Id = 0,
                        VisitorCount = tempCount.VisitorCount,
                        VisitorCountHundreds = hundoCount.VisitorCountHundreds + 1,
                        VisitorDate = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task<bool> SubmitBlog(NewBlogModel blog)
        {
            return await JsnoRepo.SubmitBlog(NewBlogModel.MapToDto(blog));
        }

        public static async Task<bool> SubmitBlog(BlogEditModel blog)
        {
            return await JsnoRepo.UpdateBlog(BlogEditModel.MapToDto(blog));
        }

        public static async Task<bool> SubmitComment(BlogCommentModel comment)
        {
            if (comment.Subscribe) await AddSubscriber(comment.Email);

            return await JsnoRepo.SubmitComment(BlogCommentModel.MapToDto(comment));
        }

        public static async Task<bool> AddSubscriber(string email)
        {
            await EmailService.NotifySnover(email);
            return await JsnoRepo.AddSubscriber(email);
        }

        public async Task NotifySnoverAboutComment(BlogCommentModel comment, BlogDisplayModel blog)
        {
            await EmailService.NotifySnover(comment, blog);
        }

        public async Task<List<BlogDisplayModel>> GetBlogs()
        {
            var blogList = new List<BlogDisplayModel>();
            blogList = await GetDataFromAzureSql();
            return blogList;
        }

        public async Task<BlogDisplayModel> GetBlog(int id)
        {
            var blogList = new List<BlogDisplayModel>();
            blogList = await GetDataFromAzureSql();
            return blogList.SingleOrDefault(blog => blog.BlogId == id);
        }

        public async Task<Blog[]> SearchBlogs(string type, string parameter)
        {
            if (db is null) await GetDataFromAzureSql();

            switch (type.ToLower())
            {
                case "topic":
                    return db.Blog.Where(blog => blog.Topic == parameter).ToArray();
                case "tag":
                    return db.Blog.Where(blog => blog.Tag.Any(tag => tag.Name == parameter)).Include(x => x.Tag).ToArray();
                case "title":
                    return db.Blog.Where(blog => blog.Title.Contains(parameter)).ToArray();
            }

            return null;
        }


        public List<BlogDisplayModel> CleanDisplayBlogs(List<BlogDisplayModel> Blogs)
        {
            Blogs = Blogs.OrderByDescending(blog => blog.SubmitDate).ToList();                        
            Blogs.ForEach(blog => 
            {
                var bodyCount = blog.Body.Count();
                if (bodyCount < 100)
                {
                    blog.Body = blog.Body
                    .Substring(0, bodyCount - 10) + @"~~ ... ";
                }
                else
                {
                    blog.Body = blog.Body
                    .Substring(0, (bodyCount < 700 ? (bodyCount - 50) : 700)) + @"~~ ... ";
                }
            });
            Blogs.ForEach(blog => blog.Body = Regex.Replace(blog.Body, @"<a[^>]*>|</a>", " "));
            Blogs.ForEach(blog => blog.Body = Regex.Replace(blog.Body, @"<a[^>]*~~", " "));
            return Blogs;
        }

        public async Task CountBlogView(int id)
        {
            try
            {
                var tempBlog = db.Blog.FirstOrDefault(c => c.Id == id);
                tempBlog.Views = tempBlog?.Views is null ? 1 : tempBlog?.Views + 1;
                db.Blog.Update(tempBlog);
                await db.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
        }
    }
}