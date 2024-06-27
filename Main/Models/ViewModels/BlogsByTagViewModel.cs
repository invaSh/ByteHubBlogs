using Main.Models.Domain;

namespace Main.Models.ViewModels
{
    public class BlogsByTagViewModel
    {
        public Tag Tag { get; set; }
        public IEnumerable<BlogPost> Blogs { get; set; }
    }
}
