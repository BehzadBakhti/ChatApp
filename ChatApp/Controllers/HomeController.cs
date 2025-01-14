using Microsoft.AspNetCore.Mvc;
using ChatApp.Models;
using System.Diagnostics;

namespace ChatApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ProcessUser([FromForm] string username, [FromForm] string avatar)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                TempData["Error"] = "Username cannot be empty!";
                return RedirectToAction("Index");
            }

            // Redirect to the Chat controller, passing the username
            return RedirectToAction("Index", "Chat", new { username, avatar });
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
