using Microsoft.AspNetCore.Mvc;
using Sabancı_ENS491_492_Website.Models;
using System.Diagnostics;

namespace Sabancı_ENS491_492_Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
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
        public IActionResult Index(bool loginError = false)
        {
            if (loginError)
            {
                // Add the error message if loginError is true
                ModelState.AddModelError("LoginError", "Invalid login attempt.");
            }

            return View();
        }

    }
}
