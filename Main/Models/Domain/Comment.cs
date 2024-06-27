using System.ComponentModel.DataAnnotations;

namespace Main.Models.Domain
{
    public class Comment
    {
        public Guid Id { get; set; }


        [Required(ErrorMessage = "Please fill in the description.")]
        public string Description { get; set; }
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }

        public DateTime DateAdded { get; set; }
        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
