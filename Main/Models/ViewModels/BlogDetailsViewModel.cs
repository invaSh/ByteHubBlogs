using Main.Models.Domain;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Main.Models.ViewModels
{
    public class BlogDetailsViewModel
    {
        public Guid Id { get; set; }
        public string Heading { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string ShortDescription { get; set; }
        public string FeaturedImageURL { get; set; }
        public string UrlHandle { get; set; }
        public DateTime PublishedDate { get; set; }
        public string Author { get; set; }
        public bool Visible { get; set; }
        public string AppUserId { get; set; }
        public AppUser AppUser { get; set; }

        public ICollection<Tag> Tags { get; set; }
        public int TotalLikes { get; set; }
    
        public bool isLiked { get; set; }

        public string CommentDescription  { get; set; }
        
        public IEnumerable<BlogComment> Comments { get; set; }

    }
}
