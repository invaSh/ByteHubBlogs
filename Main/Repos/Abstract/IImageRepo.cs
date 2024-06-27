namespace Main.Repos
{
    public interface IImageRepo
    {
        public Task<string> UploadAsync(IFormFile file);
    }
}
