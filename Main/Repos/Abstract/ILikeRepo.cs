using Main.Models.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Main.Repos
{
    public interface ILikeRepo
    {
        Task<Like> GetByUserAndBlogPostAsync(string userId, Guid blogPostId);
        Task AddAsync(Like like);
        Task RemoveAsync(Like like);
        Task<int> GetLikeCountForBlogPostAsync(Guid blogPostId);
        Task<bool> IsLikedByUserAsync(string userId, Guid blogPostId);
        Task<IEnumerable<Like>> GetLikesForBlog(Guid blogPostId);
        Task<IEnumerable<Like>> GetLikesForUserAsync(string userId);
    }
}
