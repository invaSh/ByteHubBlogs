using Main.Models.Domain;
using Main.Models.ViewModels;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;


namespace Main.Controllers
{
    [Authorize]
    public class BlogsController : Controller
    {
        private readonly ITagRepo tagRepo;
        private readonly IBlogPostRepo blogPostRepo;
        private readonly ILikeRepo likeRepo;

        private readonly UserManager<AppUser> userManager;

        public BlogsController(ITagRepo tagRepo, IBlogPostRepo blogPostRepo, ILikeRepo likeRepo, UserManager<AppUser> userManager)
        {
            this.tagRepo = tagRepo;
            this.blogPostRepo = blogPostRepo;
            this.likeRepo = likeRepo;
            this.userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var tags = await tagRepo.GetAllAsync();

            var model = new AddBlogPostRequest
            {
                Tags = tags.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AddBlogPostRequest adBr)
        {
            SaveInputToSession(adBr);

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var appUser = await userManager.FindByIdAsync(currentUserId);

            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(adBr.Heading) ||
               string.IsNullOrWhiteSpace(adBr.Title) ||
               string.IsNullOrWhiteSpace(adBr.Content) ||
               string.IsNullOrWhiteSpace(adBr.ShortDescription) ||
               string.IsNullOrWhiteSpace(adBr.FeaturedImageURL) ||
               string.IsNullOrWhiteSpace(adBr.UrlHandle))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return RedirectToAction("Add");
            }
            var blogPost = new BlogPost
            {
                Heading = adBr.Heading,
                Title = adBr.Title,
                Content = adBr.Content,
                ShortDescription = adBr.ShortDescription,
                FeaturedImageURL = adBr.FeaturedImageURL,
                UrlHandle = adBr.UrlHandle,
                PublishedDate = DateTime.Now,
                Author = appUser.UserName,
                Visible = adBr.Visible,
                AppUserId = currentUserId
            };

            var selectedTags = new List<Tag>();

            foreach (var select in adBr.SelectedTags)
            {
                if (Guid.TryParse(select, out var tagId))
                {
                    var tag = await tagRepo.GetAsync(tagId);

                    if (tag != null)
                    {
                        selectedTags.Add(tag);
                    }
                }
            }

            blogPost.BlogPostTags = selectedTags.Select(tag =>
            {
                var blogPostTag = new BlogPostTag
                {
                    BlogPost = blogPost,
                    Tag = tag
                };

                return blogPostTag;
            }).ToList();

            await blogPostRepo.AddAsync(blogPost);
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        private void SaveInputToSession(AddBlogPostRequest adBr)
        {
            if (adBr != null)
            {
                HttpContext.Session.SetString("Heading", adBr.Heading ?? string.Empty);
                HttpContext.Session.SetString("Title", adBr.Title ?? string.Empty);
                HttpContext.Session.SetString("Content", adBr.Content ?? string.Empty);
                HttpContext.Session.SetString("Author", adBr.Author ?? string.Empty);
                HttpContext.Session.SetString("ShortDescription", adBr.ShortDescription ?? string.Empty);
                HttpContext.Session.SetString("FeaturedImageURL", adBr.FeaturedImageURL ?? string.Empty);
                HttpContext.Session.SetString("UrlHandle", adBr.UrlHandle ?? string.Empty);

                var selectedTagsString = JsonConvert.SerializeObject(adBr.SelectedTags);
                HttpContext.Session.SetString("SelectedTags", selectedTagsString);
            }
        }



        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var blogPosts = await blogPostRepo.GetAllAsync();
            return View(blogPosts);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var blogPost = await blogPostRepo.GetAsync(id);
            var tagsDomainModel = await tagRepo.GetAllAsync();

            if (blogPost != null)
            {
                var model = new EditBlogPostRequest
                {
                    Id = blogPost.Id,
                    Heading = blogPost.Heading,
                    Title = blogPost.Title,
                    Content = blogPost.Content,
                    Author = blogPost.Author,
                    ShortDescription = blogPost.ShortDescription,
                    FeaturedImageURL = blogPost.FeaturedImageURL,
                    PublishedDate = blogPost.PublishedDate,
                    UrlHandle = blogPost.UrlHandle,
                    Visible = blogPost.Visible,
                    Tags = tagsDomainModel.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                    SelectedTags = blogPost.BlogPostTags.Select(x => x.TagId.ToString()).ToArray()
                };

                return View(model);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Edit(EditBlogPostRequest editBlogPostRequest)
        {

            if (string.IsNullOrWhiteSpace(editBlogPostRequest.Heading) ||
               string.IsNullOrWhiteSpace(editBlogPostRequest.Title) ||
               string.IsNullOrWhiteSpace(editBlogPostRequest.Content) ||
               string.IsNullOrWhiteSpace(editBlogPostRequest.Author) ||
               string.IsNullOrWhiteSpace(editBlogPostRequest.ShortDescription) ||
               string.IsNullOrWhiteSpace(editBlogPostRequest.FeaturedImageURL) ||
               string.IsNullOrWhiteSpace(editBlogPostRequest.UrlHandle))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return RedirectToAction("Edit", new { id = editBlogPostRequest.Id });

            }

            var blogPostDomainModel = new BlogPost
            {
                Id = editBlogPostRequest.Id,
                Heading = editBlogPostRequest.Heading,
                Title = editBlogPostRequest.Title,
                Content = editBlogPostRequest.Content,
                Author = editBlogPostRequest.Author,
                ShortDescription = editBlogPostRequest.ShortDescription,
                FeaturedImageURL = editBlogPostRequest.FeaturedImageURL,
                PublishedDate = editBlogPostRequest.PublishedDate,
                UrlHandle = editBlogPostRequest.UrlHandle,
                Visible = editBlogPostRequest.Visible
            };

            var selectedTags = new List<BlogPostTag>();

            foreach (var selectedTag in editBlogPostRequest.SelectedTags)
            {
                if (Guid.TryParse(selectedTag, out var tagId))
                {
                    var foundTag = await tagRepo.GetAsync(tagId);

                    if (foundTag != null)
                    {
                        selectedTags.Add(new BlogPostTag
                        {
                            BlogPost = blogPostDomainModel,
                            Tag = foundTag
                        });
                    }
                }
            }

            blogPostDomainModel.BlogPostTags = selectedTags;

            var updatedBlog = await blogPostRepo.UpdateAsync(blogPostDomainModel);

            if (updatedBlog != null)
            {
                return RedirectToAction("Details", new { id = updatedBlog.Id });
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deletedPost = await blogPostRepo.DeleteAsync(id);

            if (deletedPost != null)
            {
                return RedirectToAction("UserProfile", "Account");
            }

            return RedirectToAction("Index");

        }

        [HttpGet("/Blogs/ByTag/{tagId}")]
        public async Task<IActionResult> BlogsByTag(Guid tagId)
        {
            var tag = await tagRepo.GetAsync(tagId);

            if (tag == null)
            {
                return NotFound();
            }

            var blogsByTag = await blogPostRepo.GetBlogsByTagAsync(tagId);

            var model = new BlogsByTagViewModel
            {
                Tag = tag,
                Blogs = blogsByTag
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var blogPost = await blogPostRepo.GetAsync(id);

            if (blogPost == null)
            {
                return NotFound();
            }

            return View(blogPost);
        }

        [HttpPost]
        [Route("Blogs/Like")]

        public async Task<IActionResult> Like(Guid blogPostId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (currentUserId == null)
            {
                return Json(new { success = false, error = "User not authenticated" });
            }

            try
            {
                var existingLike = await likeRepo.GetByUserAndBlogPostAsync(currentUserId, blogPostId);

                if (existingLike == null)
                {
                    var like = new Like
                    {
                        UserId = currentUserId,
                        BlogPostId = blogPostId
                    };

                    await likeRepo.AddAsync(like);
                }
                else
                {
                    await likeRepo.RemoveAsync(existingLike);
                }

                var updatedLikeCount = await likeRepo.GetLikeCountForBlogPostAsync(blogPostId);

                return Json(new { success = true, likeCount = updatedLikeCount, isLiked = (existingLike == null) });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Like action: {ex.Message}");

                return Json(new { success = false, error = "An error occurred while processing the like." });
            }
        }

        [HttpGet]
        [Route("Blogs/GetLikes")]
        public async Task<IActionResult> GetLikes(Guid blogPostId)
        {
            try
            {
                var likeCount = await likeRepo.GetLikeCountForBlogPostAsync(blogPostId);

                var isLiked = await likeRepo.IsLikedByUserAsync(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, blogPostId);
                return Json(new { success = true, likeCount, isLiked });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetLikes action: {ex.Message}");
                return Json(new { success = false, error = "An error occurred while getting the like count." });
            }
        }


    }
}


