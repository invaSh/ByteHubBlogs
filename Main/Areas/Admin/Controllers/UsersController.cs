using Main.Models.Domain;
using Main.Models.ViewModels;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/[controller]/[action]")]
    [Authorize(Roles = "Admin, Head Admin")]

    public class UsersController : Controller
    {
        private IUserRepo UserRepo { get; }
        private readonly IBlogPostRepo blogPostRepo;
        private readonly ICommentRepo commentRepo;
        private UserManager<AppUser> UserManager { get; }

        public UsersController(IUserRepo userRepo, UserManager<AppUser> userManager, IBlogPostRepo blogPostRepo, ICommentRepo commentRepo)
        {
            UserRepo = userRepo;
            UserManager = userManager;
            this.blogPostRepo = blogPostRepo;
            this.commentRepo = commentRepo;
        }

        [HttpGet]
        public async Task<IActionResult> List(bool? isAdmin, bool? isUser, int page = 1, int pageSize = 5)
        {
            ViewBag.IsAdmin = isAdmin ?? false;
            ViewBag.IsUser = isUser ?? false;

            var usersViewModel = new UserList();
            usersViewModel.Users = new List<User>();

            if (isAdmin.HasValue && isAdmin.Value && (!isUser.HasValue || !isUser.Value))
            {
                var adminUsers = await UserRepo.GetByRoleAsync("Admin");
                foreach (var user in adminUsers)
                {
                    usersViewModel.Users.Add(new User
                    {
                        Id = Guid.Parse(user.Id),
                        Username = user.UserName,
                        EmailAddress = user.Email
                    });
                }
            }
            else if (isUser.HasValue && isUser.Value && (!isAdmin.HasValue || !isAdmin.Value))
            {
                var regularUsers = await UserRepo.GetByRoleAsync("User");
                foreach (var user in regularUsers)
                {
                    usersViewModel.Users.Add(new User
                    {
                        Id = Guid.Parse(user.Id),
                        Username = user.UserName,
                        EmailAddress = user.Email
                    });
                }
            }
            else
            {
                var users = await UserRepo.GetAll();
                foreach (var user in users)
                {
                    usersViewModel.Users.Add(new User
                    {
                        Id = Guid.Parse(user.Id),
                        Username = user.UserName,
                        EmailAddress = user.Email
                    });
                }
            }

            // Paginate the results
            var totalCount = usersViewModel.Users.Count;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;

            var paginatedUsers = usersViewModel.Users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            usersViewModel.Users = paginatedUsers;

            return View(usersViewModel);
        }


        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreateUserViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {

            HttpContext.Session.SetString("UsernameData", model.Username ?? string.Empty);
            HttpContext.Session.SetString("EmailData", model.Email ?? string.Empty);
            HttpContext.Session.SetString("PasswordData", model.Password ?? string.Empty);

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                };

                HttpContext.Session.Clear();
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (model.IsAdmin)
                    {
                        await UserManager.AddToRoleAsync(user, "Admin");
                    }
                    else if (model.IsHeadAdmin)
                    {
                        await UserManager.AddToRoleAsync(user, "Head Admin");
                    }
                    else if (model.IsUser)
                    {
                        await UserManager.AddToRoleAsync(user, "User");
                    }

                    return RedirectToAction(nameof(List));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }



        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = new UserDetailsViewModel
            {
                Id = Guid.Parse(user.Id),
                Username = user.UserName,
                EmailAddress = user.Email,
                Role = GetUserRole(user)
            };

            return View(userViewModel);
        }

        private string GetUserRole(AppUser user)
        {
            if (user != null)
            {
                if (UserManager.IsInRoleAsync(user, "Admin").Result)
                {
                    return "Admin";
                }
                else if (UserManager.IsInRoleAsync(user, "Head Admin").Result)
                {
                    return "Head Admin";
                }
            }

            return "User";
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var user = await UserManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var userViewModel = new UserDetailsViewModel
            {
                Id = Guid.Parse(user.Id),
                Username = user.UserName,
                EmailAddress = user.Email,
                Role = GetUserRole(user)
            };

            return View(userViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDetailsViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByIdAsync(model.Id.ToString());

                if (user == null)
                {
                    return NotFound();
                }

                await blogPostRepo.UpdateUsernameInBlogPostsAsync(user.Id, model.Username);
                await commentRepo.UpdateUsernameInCommentsAsync(user.Id, model.Username);
                user.UserName = model.Username;
                user.Email = model.EmailAddress;

                var result = await UserManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var roles = await UserManager.GetRolesAsync(user);
                    await UserManager.RemoveFromRolesAsync(user, roles);

                    switch (model.Role)
                    {
                        case "Admin":
                            await UserManager.AddToRoleAsync(user, "Admin");
                            break;

                        case "Head Admin":
                            await UserManager.AddToRoleAsync(user, "Head Admin");
                            break;

                        case "User":
                            await UserManager.AddToRoleAsync(user, "User");
                            break;


                        default:
                            break;
                    }

                    return RedirectToAction(nameof(Details), new { id = model.Id });
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(string id)
        {
            await UserRepo.DeleteUser(id);
            return RedirectToAction("List", "Users", new { area = "Admin" });
        }
    }
}