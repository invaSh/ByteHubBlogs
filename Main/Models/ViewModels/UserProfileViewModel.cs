using Main.Models.Domain;
using System.ComponentModel.DataAnnotations;

namespace Main.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public string Id { get; set; }
        public string Username { get; set; }

        public string EmailAddress { get; set; }

        public ICollection<BlogPost> BlogPosts { get; set; }
    }
}
