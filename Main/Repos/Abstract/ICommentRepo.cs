using Main.Models.Domain;

namespace Main.Repos
{
    public interface ICommentRepo
    {
        Task<Comment> GetByIdAsync(Guid id);
        Task<IEnumerable<Comment>> GetAllAsync();
        Task<IEnumerable<Comment>> GetCommentsForPostAsync(Guid postId);
        Task AddAsync(Comment comment);
        Task UpdateAsync(Comment comment);
        Task DeleteAsync(Guid id);
        Task UpdateUsernameInCommentsAsync(string userId, string newUsername);
        Task<IEnumerable<Comment>> GetCommentsForUserAsync(string userId);
    }
}
