using Main.Models.Domain;

namespace Main.Repos
{
    public interface IBlogPostRepo
    {
        Task<IEnumerable<BlogPost>> GetAllAsync();
        Task<BlogPost?> GetAsync(Guid id);
        Task<BlogPost?> GetByUrlHandleAsync(string UrlHandle);
        Task<BlogPost> AddAsync(BlogPost post);
        Task<BlogPost?> UpdateAsync(BlogPost post);
        Task<BlogPost?> DeleteAsync(Guid id);
        Task<IEnumerable<BlogPost>> GetBlogsByTagAsync(Guid tagId);
        Task<IEnumerable<BlogPost>> GetBlogPostsByUserId(string userId);
        Task UpdateUsernameInBlogPostsAsync(string userId, string newUsername);
        Task<IEnumerable<BlogPost>> GetBlogsByTagAsync(string query);

        Task<IEnumerable<BlogPost>> SearchAsync(string query);
       
    }
}
