using Main.Models.Domain;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Main.Controllers
{
    
    public class CommentsController : Controller
    {
        private readonly ICommentRepo _commentRepo;
        private readonly UserManager<AppUser> _userManager;
        private readonly IBlogPostRepo _blogPostRepo;

        public CommentsController(ICommentRepo commentRepository, UserManager<AppUser> userManager, IBlogPostRepo blogPostRepo)
        {
            _commentRepo = commentRepository;
            _userManager = userManager;
            _blogPostRepo = blogPostRepo;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid postId, string description)
        {

            HttpContext.Session.SetString("TempComment", description ?? string.Empty);
            var currentUser = await _userManager.GetUserAsync(User);
            var blogPost = await _blogPostRepo.GetAsync(postId);

            if (description == null || description.Trim() == "")
            {
                TempData["ErrorMessage"] = "All fields are required.";
            }

            var comment = new Comment
            {
                Description = description,
                DateAdded = DateTime.Now,
                BlogPostId = postId,
                UserId = currentUser.Id,
                AppUser = currentUser,
                BlogPost = blogPost
            };

            await _commentRepo.AddAsync(comment);
            HttpContext.Session.Clear();
            return RedirectToAction("Details", "Blogs", new { id = postId });
        }


        public async Task<IActionResult> Edit(Guid id, string description)
        {
            var comment = await _commentRepo.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (comment.UserId != currentUser.Id)
            {
                return Forbid();
            }

            comment.Description = description;
            await _commentRepo.UpdateAsync(comment);

            return RedirectToAction("Details", "Blogs", new { id = comment.BlogPostId });
        }

        [HttpPost]
        [Authorize(Roles = "Head Admin, User")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var comment = await _commentRepo.GetByIdAsync(id);

            if (comment == null)
            {
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (comment.UserId != currentUser.Id && !User.IsInRole("Head Admin"))
            {
                return Forbid();
            }

            await _commentRepo.DeleteAsync(id);

            return RedirectToAction("Details", "Blogs", new { id = comment.BlogPostId });
        }



    }
}
