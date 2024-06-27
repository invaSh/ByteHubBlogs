using Main.Data;
using Main.Models.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Main.Repos
{
    public class UserRepo : IUserRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public UserRepo(ApplicationDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<IdentityUser>> GetAll()
        {
            var users = await _context.Users.ToListAsync();

            var headAdminRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == "Head Admin");

            if (headAdminRole != null)
            {
                var headAdminUsers = await _context.UserRoles
                    .Where(ur => ur.RoleId == headAdminRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                users = users.Where(u => !headAdminUsers.Contains(u.Id)).ToList();
            }

            return users;
        }

        public async Task<IdentityUser> GetById(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task Create(IdentityUser user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task Update(IdentityUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(string userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<AppUser>> GetByRoleAsync(string roleName)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(roleName);
            return usersInRole.ToList();
        }

        public async Task DeleteUser(string userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user != null)
            {
                var blogPosts = await _context.BlogPosts
                    .Where(bp => bp.AppUserId == userId)
                    .ToListAsync();
                _context.BlogPosts.RemoveRange(blogPosts);

                var comments = await _context.Comments
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                _context.Comments.RemoveRange(comments);

                var likes = await _context.Likes
                    .Where(l => l.UserId == userId)
                    .ToListAsync();
                _context.Likes.RemoveRange(likes);

                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
            }
        }

    }
}
