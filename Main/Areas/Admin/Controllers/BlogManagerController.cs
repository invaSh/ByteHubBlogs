using Main.Models.Domain;
using Main.Models.ViewModels;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin, Head Admin")]
    public class BlogManagerController : Controller
    {
        private readonly ITagRepo tagRepo;
        private readonly IBlogPostRepo blogPostRepo;

        public BlogManagerController(ITagRepo tagRepo, IBlogPostRepo blogPostRepo)
        {
            this.tagRepo = tagRepo;
            this.blogPostRepo = blogPostRepo;
        }

        private async Task<IEnumerable<BlogPost>> GetPaginatedBlogPosts(Guid? tagId, int page, int pageSize)
        {
            var blogPosts = tagId.HasValue
                ? await blogPostRepo.GetBlogsByTagAsync(tagId.Value)
                : await blogPostRepo.GetAllAsync();

            return blogPosts.Skip((page - 1) * pageSize).Take(pageSize).ToList();
        }

        [HttpGet]
        [ActionName("List")]
        public async Task<IActionResult> List(Guid? tagId, int page = 1, int pageSize = 6)
        {
            var tags = await tagRepo.GetAllAsync();
            ViewBag.Tags = tags;

            IEnumerable<BlogPost> blogPosts;

            if (tagId.HasValue)
            {
                blogPosts = await blogPostRepo.GetBlogsByTagAsync(tagId.Value);
            }
            else
            {
                blogPosts = await blogPostRepo.GetAllAsync();
            }

            // Calculate total pages
            var totalCount = blogPosts.Count();
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            // Paginate the results
            var paginatedBlogs = await GetPaginatedBlogPosts(tagId, page, pageSize);

            return View(paginatedBlogs);
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

        [HttpGet("/Admin/BlogManager/Edit/{id}")]
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
                    FeaturedImageURL = blogPost.FeaturedImageURL,
                    UrlHandle = blogPost.UrlHandle,
                    ShortDescription = blogPost.ShortDescription,
                    PublishedDate = blogPost.PublishedDate,
                    Visible = blogPost.Visible,
                    Tags = tagsDomainModel.Select(x => new SelectListItem
                    {
                        Text = x.Name,
                        Value = x.Id.ToString()
                    }),
                    SelectedTags = blogPost.BlogPostTags.Select(x => x.Tag.Id.ToString()).ToArray()
                };

                return View(model);
            }

            return NotFound();
        }

        [HttpPost("/Admin/BlogManager/Update/{id}")]
        public async Task<IActionResult> Update(Guid id, EditBlogPostRequest editBlogPostRequest)
        {
            if (string.IsNullOrWhiteSpace(editBlogPostRequest.Heading) ||
                string.IsNullOrWhiteSpace(editBlogPostRequest.Title) ||
                string.IsNullOrWhiteSpace(editBlogPostRequest.Content) ||
                string.IsNullOrWhiteSpace(editBlogPostRequest.ShortDescription) ||
                string.IsNullOrWhiteSpace(editBlogPostRequest.FeaturedImageURL) ||
                string.IsNullOrWhiteSpace(editBlogPostRequest.UrlHandle))
            {
                TempData["ErrorMessage"] = "All fields are required.";
                return RedirectToAction("Edit", new { id = editBlogPostRequest.Id });
            }

            var blogPostDomainModel = new BlogPost
            {
                Id = id,
                Heading = editBlogPostRequest.Heading,
                Title = editBlogPostRequest.Title,
                Content = editBlogPostRequest.Content,
                Author = editBlogPostRequest.Author,
                ShortDescription = editBlogPostRequest.ShortDescription,
                FeaturedImageURL = editBlogPostRequest.FeaturedImageURL,
                PublishedDate = DateTime.Now,
                UrlHandle = editBlogPostRequest.UrlHandle,
                Visible = editBlogPostRequest.Visible
            };

            var selectedTags = new List<BlogPostTag>();
            foreach (var selectedTagId in editBlogPostRequest.SelectedTags)
            {
                if (Guid.TryParse(selectedTagId, out var tagId))
                {
                    var foundTag = await tagRepo.GetAsync(tagId);

                    if (foundTag != null)
                    {
                        var blogPostTag = new BlogPostTag
                        {
                            TagId = foundTag.Id,
                            Tag = foundTag,
                            BlogPostId = blogPostDomainModel.Id,
                            BlogPost = blogPostDomainModel
                        };

                        selectedTags.Add(blogPostTag);
                    }
                }
            }

            blogPostDomainModel.BlogPostTags = selectedTags;

            var updatedBlog = await blogPostRepo.UpdateAsync(blogPostDomainModel);

            if (updatedBlog != null)
            {
                return RedirectToAction("Edit", new { id = updatedBlog.Id });
            }

            TempData["SuccessMessage"] = "Blog post updated successfully.";
            return RedirectToAction("Edit", new { id = updatedBlog.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deletedPost = await blogPostRepo.DeleteAsync(id);

            if (deletedPost != null)
            {
                TempData["SuccessMessage"] = "Blog post deleted successfully.";
                return RedirectToAction("List");
            }

            TempData["ErrorMessage"] = "Error deleting the blog post.";

            return RedirectToAction("Edit", new { id });
        }
    }
}
