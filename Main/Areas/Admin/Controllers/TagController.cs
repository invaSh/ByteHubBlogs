using Main.Data;
using Main.Models.Domain;
using Main.Models.ViewModels;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Head Admin, Admin")]
    public class TagController : Controller
    {
        private readonly ITagRepo _db;

        public TagController(ITagRepo db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Add")]
        public async Task<IActionResult> Add(AddTagRequest addTagRequest)
        {

            HttpContext.Session.SetString("TagName", addTagRequest.Name ?? string.Empty);
            HttpContext.Session.SetString("TagDisplay", addTagRequest.DisplayName ?? string.Empty);

            if (!ModelState.IsValid)
            {
                return View(addTagRequest);
            }

            var tag = new Tag
            {
                Name = addTagRequest.Name,
                DisplayName = addTagRequest.DisplayName
            };

            await _db.AddAsync(tag);
            HttpContext.Session.Clear();
            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> List(int page = 1, int pageSize = 5)
        {
            var tags = await _db.GetAllAsync();

            // Check if tags is not null
            if (tags != null)
            {
                // Calculate total pages
                var totalCount = tags.Count();
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                ViewBag.TotalPages = totalPages;
                ViewBag.CurrentPage = page;

                // Paginate the results
                var paginatedTags = tags.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                return View(paginatedTags);
            }
            else
            {
                // Handle the case where tags is null
                ViewBag.TotalPages = 0;
                ViewBag.CurrentPage = 0;
                return View(new List<Tag>());
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var tag = await _db.GetAsync(id);

            if (tag != null)
            {
                var editTagReq = new EditTagRequest
                {
                    Id = tag.Id,
                    Name = tag.Name,
                    DisplayName = tag.DisplayName
                };

                return View(editTagReq);
            }

            return View("NotFound");
        }

        [HttpPost("/Admin/Tag/Edit/{id}")]
        [ActionName("Update")]
        public async Task<IActionResult> UpdateTag(Guid id, EditTagRequest editTagRequest)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", editTagRequest);
            }

            var tag = new Tag
            {
                Id = id,
                Name = editTagRequest.Name,
                DisplayName = editTagRequest.DisplayName
            };

            var updatedTag = await _db.UpdateAsync(tag);

            if (updatedTag != null)
            {
                return RedirectToAction("List");
            }

            ModelState.AddModelError(string.Empty, "Failed to update the tag. Please try again.");
            return View("Edit", editTagRequest);
        }

        [HttpPost("/Admin/Tag/Details/{id}")]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(Guid id, EditTagRequest eDT)
        {
            var deleted = await _db.DeleteAsync(id);

            if (deleted != null)
            {
                return RedirectToAction("List");
            }

            return RedirectToAction("Edit", new { id = eDT.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var tag = await _db.GetAsync(id);

            if (tag != null)
            {
                return View(tag);
            }

            return View("NotFound");
        }
    }
}
