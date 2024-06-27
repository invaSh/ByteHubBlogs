using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Main.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Head Admin")]
    public class MainController : Controller
    {
        [HttpGet("/admin")]
        public IActionResult MainPage()
        {
            return View();
        }
    }
}
