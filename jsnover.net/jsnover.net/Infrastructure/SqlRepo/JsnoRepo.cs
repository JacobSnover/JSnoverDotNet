using jsnover.net.Constants;
using jsnover.net.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace jsnover.net.Infrastructure.SqlRepo
{
    public static class JsnoRepo
    {
        public static async Task<bool> SubmitBlog(Blog[] blogs)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.Blog.Add(blogs[BlogIndexes.Blog]);
                await db.SaveChangesAsync();

                blogs[BlogIndexes.Photos].Photos.ToList().ForEach(photo => photo.BlogId = blogs[BlogIndexes.Blog].Id);
                await db.Photos.AddRangeAsync(blogs[BlogIndexes.Photos].Photos);
                await db.SaveChangesAsync();

                var blogTags = blogs[BlogIndexes.Tags].Tag.ToList();
                for (int i = 0; i < blogs[BlogIndexes.Tags].Tag.Count; i++)
                {
                    await db.Tag.AddAsync(new Tag
                    {
                        Id = 0,
                        BlogId = blogs[BlogIndexes.Blog].Id,
                        Name = blogTags[i].Name
                    });
                }
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        internal static async Task<bool> UpdateBlog(Blog[] blogs)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.Blog.Update(blogs[BlogIndexes.Blog]);
                await db.SaveChangesAsync();

                db.Photos.UpdateRange(blogs[BlogIndexes.Photos].Photos);
                await db.SaveChangesAsync();
                await db.Photos.AddRangeAsync(blogs[BlogIndexes.NewPhotos].Photos);
                await db.SaveChangesAsync();
                db.Photos.RemoveRange(blogs[BlogIndexes.RemovePhotos].Photos);
                await db.SaveChangesAsync();

                db.Tag.UpdateRange(blogs[BlogIndexes.Tags].Tag);
                await db.SaveChangesAsync();
                await db.Tag.AddRangeAsync(blogs[BlogIndexes.NewTags].Tag);
                await db.SaveChangesAsync();
                db.Tag.RemoveRange(blogs[BlogIndexes.RemoveTags].Tag);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal async static Task<bool> SubmitComment(Commentors commentors)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                var blog = db.Blog.First(b => b.Id == commentors.BlogId);
                db.Commentors.Add(commentors);
                if (commentors.Liked)
                {
                    blog.Likes = blog.Likes is null ? 1 : (blog.Likes++);
                    db.Blog.Update(blog);
                }
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        internal async static Task<bool> AddSubscriber(string email)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();

                if (db.Subscribers.Where(sub => sub.Email == email).Count() == 0)
                {
                    db.Subscribers.Add(new Subscribers()
                    {
                        Id = 0,
                        Email = email,
                        SubscribeDate = DateTime.Now
                    });
                    await db.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static async Task<bool> SubmitContactRequest(ContactRequest contactRequest)
        {
            try
            {
                using var db = new jsnoverdotnetdbContext();
                db.ContactRequest.Add(contactRequest);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
