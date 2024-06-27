using Main.Models.Domain;
using Main.Models.ViewModels;
using Main.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace Main.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IBlogPostRepo _blogPostRepo;
        private readonly ICommentRepo _commentRepo;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> _signInManager, ILogger<HomeController> logger, IBlogPostRepo blogPostRepo, ICommentRepo commentRepo)
        {
            _logger = logger;
            _blogPostRepo = blogPostRepo;
            this.userManager = userManager;
            signInManager = _signInManager;
            _commentRepo = commentRepo;
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByNameAsync(registerViewModel.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(RegisterViewModel.Username), "Username already exists.");
                    return View(registerViewModel);
                }

                existingUser = await userManager.FindByEmailAsync(registerViewModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(nameof(RegisterViewModel.Email), "Email is already registered.");
                    return View(registerViewModel);
                }

                if (!IsPasswordValid(registerViewModel.Password))
                {
                    ModelState.AddModelError(nameof(RegisterViewModel.Password), "Password must be at least 6 characters long and contain at least one capital letter and one number.");
                    return View(registerViewModel);
                }

                var identityUser = new AppUser
                {
                    UserName = registerViewModel.Username,
                    Email = registerViewModel.Email
                };

                var identityResult = await userManager.CreateAsync(identityUser, registerViewModel.Password);

                if (identityResult.Succeeded)
                {
                    var roleIdentityResult = await userManager.AddToRoleAsync(identityUser, "User");

                    if (roleIdentityResult.Succeeded)
                    {
                        return RedirectToAction("Login");
                    }
                }
            }

            return View();
        }

        private bool IsPasswordValid(string password)
        {
            return Regex.IsMatch(password, @"^(?=.*[A-Z])(?=.*\d).{6,}$");
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userManager.FindByNameAsync(model.Username);

                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "User not found.");
                    return View(model);
                }

                if (signInManager == null)
                {
                    ModelState.AddModelError(string.Empty, "Sign in manager is null.");
                    return View(model);
                }

                var result = await signInManager.PasswordSignInAsync(user.UserName, model.Password, false, false);

                if (result.Succeeded)
                {
                    if (await userManager.IsInRoleAsync(user, "Admin") || await userManager.IsInRoleAsync(user, "Head Admin"))
                    {
                        return RedirectToAction("MainPage", "Main", new { area = "Admin" });
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Incorrect username or password.");
                    return View(model);
                }
            }

            return View();
        }




        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UserProfile()
        {
            var user = await userManager.GetUserAsync(User);

            if (user == null)
            {
                _logger.LogError("User not found.");
                return NotFound();
            }

            var allBlogPosts = await _blogPostRepo.GetAllAsync();

            var userN = await userManager.GetUserAsync(User);
            var userBlogPosts = allBlogPosts.Where(post => post.AppUserId == user.Id).ToList();

            var userProfileViewModel = new UserProfileViewModel
            {
                Id=user.Id,
                Username = user.UserName,
                EmailAddress = user.Email,
                BlogPosts = userBlogPosts
            };

            return View(userProfileViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(string id)
        {
            var user = await userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new EditUserProfileViewModel
            {
                Username = user.UserName,
                EmailAddress = user.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditUserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            await _blogPostRepo.UpdateUsernameInBlogPostsAsync(user.Id, model.Username);
            await _commentRepo.UpdateUsernameInCommentsAsync(user.Id, model.Username);

            user.UserName = model.Username;
            user.Email = model.EmailAddress;

            var result = await userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                return RedirectToAction("UserProfile", new { id = model.Id });
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }
        }

    }
}
