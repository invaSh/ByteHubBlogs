using Main.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Main.Repos
{
    public interface IUserRepo
    {
        Task<IEnumerable<IdentityUser>> GetAll();
        Task<IdentityUser> GetById(string userId);
        Task Create(IdentityUser user);
        Task Update(IdentityUser user);
        Task Delete(string userId);
        Task<List<AppUser>> GetByRoleAsync(string roleName);

        Task DeleteUser(string userId);
    }
}
