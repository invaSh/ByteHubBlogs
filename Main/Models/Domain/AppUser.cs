using Microsoft.AspNetCore.Identity;

namespace Main.Models.Domain
{
    public class AppUser : IdentityUser
    {
        public ICollection<BlogPost> BlogPosts { get; set; }

        public ICollection<Comment> Comments { get; set; }

        public ICollection<Like> Likes { get; set; }
    }
}
