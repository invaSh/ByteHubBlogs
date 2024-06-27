using Main.Data;
using Main.Models.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main.Repos.Concrete
{
    public class BlogPostRepo : IBlogPostRepo
    {
        private readonly ApplicationDbContext _context;

        public BlogPostRepo(ApplicationDbContext db)
        {
            _context = db;
        }

        public async Task<BlogPost> AddAsync(BlogPost post)
        {
            await _context.AddAsync(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<BlogPost> DeleteAsync(Guid id)
        {
            var blogPost = await _context.BlogPosts
                .Include(bp => bp.Comments)
                .Include(bp => bp.Likes)
                .FirstOrDefaultAsync(bp => bp.Id == id);

            if (blogPost == null)
            {
                return null;
            }

            _context.Comments.RemoveRange(blogPost.Comments);

            _context.Likes.RemoveRange(blogPost.Likes);

            _context.BlogPosts.Remove(blogPost);

            await _context.SaveChangesAsync();

            return blogPost;
        }


        public IQueryable<BlogPost> GetAll()
        {
            return _context.BlogPosts
                .Include(b => b.BlogPostTags)
                .ThenInclude(bpt => bpt.Tag);
        }

        public async Task<IEnumerable<BlogPost>> GetAllAsync()
        {
            return await GetAll().ToListAsync();
        }

        public async Task<BlogPost?> GetAsync(Guid id)
        {
            return await _context.BlogPosts
                .Include(x => x.BlogPostTags)
                .ThenInclude(bpt => bpt.Tag)
                .Include(x => x.Comments)
                .ThenInclude(c => c.AppUser)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<BlogPost?> GetByUrlHandleAsync(string urlHandle)
        {
            return await _context.BlogPosts
                .Include(x => x.BlogPostTags)
                .FirstOrDefaultAsync(x => x.UrlHandle == urlHandle);
        }

        public async Task<BlogPost?> UpdateAsync(BlogPost blogPost)
        {
            var existingBlog = await _context.BlogPosts
                .Include(x => x.BlogPostTags)
                .FirstOrDefaultAsync(x => x.Id == blogPost.Id);

            if (existingBlog != null)
            {
                existingBlog.Id = blogPost.Id;
                existingBlog.Heading = blogPost.Heading;
                existingBlog.Title = blogPost.Title;
                existingBlog.Content = blogPost.Content;
                existingBlog.ShortDescription = blogPost.ShortDescription;
                existingBlog.Author = blogPost.Author;
                existingBlog.FeaturedImageURL = blogPost.FeaturedImageURL;
                existingBlog.UrlHandle = blogPost.UrlHandle;
                existingBlog.Visible = blogPost.Visible;
                existingBlog.PublishedDate = blogPost.PublishedDate;
                existingBlog.BlogPostTags = blogPost.BlogPostTags;

                await _context.SaveChangesAsync();
                return existingBlog;
            }

            return null;
        }

        public async Task<IEnumerable<BlogPost>> GetBlogsByTagAsync(Guid tagId)
        {
            return await _context.BlogPosts
                .Include(bp => bp.BlogPostTags)
                .Where(blogPost => blogPost.BlogPostTags.Any(bpt => bpt.TagId == tagId))
                .ToListAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetBlogPostsByUserId(string userId)
        {
            return await _context.BlogPosts
                .Where(blogPost => blogPost.AppUserId == userId)
                .ToListAsync();
        }

        public async Task UpdateUsernameInBlogPostsAsync(string userId, string newUsername)
        {
            var blogPosts = await _context.BlogPosts
                .Where(bp => bp.AppUserId == userId)
                .ToListAsync();

            foreach (var blogPost in blogPosts)
            {
                blogPost.Author = newUsername;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<BlogPost>> GetBlogsByTagAsync(string query)
        {
            return await _context.BlogPosts
                .Include(bp => bp.BlogPostTags)
                .Where(blogPost => blogPost.BlogPostTags.Any(bpt => bpt.Tag.Name == query || bpt.Tag.DisplayName == query))
                .ToListAsync();
        }


        public async Task<IEnumerable<BlogPost>> SearchAsync(string query)
        {
            return await _context.BlogPosts
                .Include(bp => bp.BlogPostTags)
                .Where(blogPost =>
                    blogPost.Title.Contains(query) ||
                    blogPost.BlogPostTags.Any(bpt => bpt.Tag.Name.Contains(query)))
                .ToListAsync();
        }
    }
}
