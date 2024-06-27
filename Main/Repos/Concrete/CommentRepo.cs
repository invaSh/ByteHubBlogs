using Main.Data;
using Main.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace Main.Repos
{
    public class CommentRepo : ICommentRepo
    {
        private readonly ApplicationDbContext _context;

        public CommentRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> GetByIdAsync(Guid id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task<IEnumerable<Comment>> GetAllAsync()
        {
            return await _context.Comments.ToListAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsForPostAsync(Guid postId)
        {
            return await _context.Comments
                .Include(c => c.AppUser)
                .Where(c => c.BlogPostId == postId)
                .ToListAsync();
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var comment = await GetByIdAsync(id);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateUsernameInCommentsAsync(string userId, string newUsername)
        {
            var comments = await _context.Comments.Where(c => c.UserId == userId).ToListAsync();

            foreach (var comment in comments)
            {
                comment.AppUser.UserName = newUsername;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetCommentsForUserAsync(string userId)
        {
            return await _context.Comments
                .Include(c => c.AppUser)
                .Where(c => c.UserId == userId)
                .ToListAsync();
        }
    }
}
