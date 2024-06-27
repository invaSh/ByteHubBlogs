namespace Main.Models.Domain
{
    public class Like
    {
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public AppUser AppUser { get; set; }
        public Guid BlogPostId { get; set; }
        public BlogPost BlogPost { get; set; }
    }
}
