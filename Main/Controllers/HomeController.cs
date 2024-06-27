using Main.Models;
using Main.Models.Domain;
using Main.Models.ViewModels;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Main.Controllers
{
    [Authorize]

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogPostRepo blogPostRepo;
        private readonly ITagRepo tagRepo;

        public HomeController(ILogger<HomeController> logger, IBlogPostRepo blogPostRepo, ITagRepo tagRepo)
        {
            _logger = logger;
            this.blogPostRepo = blogPostRepo;
            this.tagRepo = tagRepo;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var blogPosts = await blogPostRepo.GetAllAsync();

            var tags = await tagRepo.GetAllAsync();

            var model = new HomeViewModel
            {
                BlogPosts = blogPosts,
                Tags = tags
            };

            return View(model);
        }


        public async Task<IActionResult> Search(string query)
        {
            IEnumerable<BlogPost> searchResults;

            if (string.IsNullOrEmpty(query))
            {
                // No search query provided, return all blog posts
                searchResults = await blogPostRepo.GetAllAsync();
            }
            else
            {
                // Implement your search logic based on the provided query
                searchResults = await blogPostRepo.GetBlogsByTagAsync(query);
            }

            var tags = await tagRepo.GetAllAsync();

            var viewModel = new HomeViewModel
            {
                Tags = tags,
                BlogPosts = searchResults
            };

            return View("Index", viewModel);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }


}