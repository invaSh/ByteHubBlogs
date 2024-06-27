using Main.Data;
using Main.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Main.Repos
{
    public class LikeRepo : ILikeRepo
    {
        private readonly ApplicationDbContext _context;
        public LikeRepo(ApplicationDbContext _context)
        {
            this._context = _context;
        }

        public async Task<Like> GetByUserAndBlogPostAsync(string userId, Guid blogPostId)
        {
            return await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.BlogPostId == blogPostId);
        }

        public async Task AddAsync(Like like)
        {
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Like like)
        {
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetLikeCountForBlogPostAsync(Guid blogPostId)
        {
            return await _context.Likes
                .Where(like => like.BlogPostId == blogPostId)
                .CountAsync();
        }

        public async Task<bool> IsLikedByUserAsync(string userId, Guid blogPostId)
        {
            var existingLike = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.BlogPostId == blogPostId);

            return existingLike != null;
        }

        public async Task<IEnumerable<Like>> GetLikesForBlog(Guid blogPostId)
        {
            return await _context.Likes.Where(x => x.BlogPostId == blogPostId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Like>> GetLikesForUserAsync(string userId)
        {
            return await _context.Likes
                .Where(like => like.UserId == userId)
                .ToListAsync();
        }

    }
}
