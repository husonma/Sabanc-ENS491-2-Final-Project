using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Sabancı_ENS491_492_Website.Data;
using Sabancı_ENS491_492_Website.Models;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Sabancı_ENS491_492_Website.Controllers
{
    public class AccountController : Controller
    {
        private readonly ProjectContext _context;

        public AccountController(ProjectContext context)
        {
            _context = context;
        }


        [HttpPost]
        public async Task<IActionResult> Login(User loginUser)
        {
            // Simulate authentication logic - replace this with your actual logic
            var user = _context.Users
                .FirstOrDefault(u => u.EmailAddress == loginUser.EmailAddress
                    && u.Password == loginUser.Password); // WARNING: Store and check hashed passwords, not plain text!

            if (user != null)
            {
                var claims = new List<Claim>{
                    new Claim(ClaimTypes.Name, user.EmailAddress),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim("FullName", user.Name)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                        // Configure your authentication properties if needed
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
                // Check the role and redirect accordingly
                if (user.Role == "student")
                {
                    return RedirectToAction("ProjectsListView", "Projects");
                }
                else if (user.Role == "instructor")
                {
                    return RedirectToAction("ProjectAddView", "Projects");
                }
                else if (user.Role == "admin")
                {
                    return RedirectToAction("ProjectAddView", "Projects");
                }
                else
                {
                    // If the role is not recognized, send them back to the login page
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                // User not found
                ModelState.AddModelError("LoginError", "Invalid login attempt.");
                // Redirect back to the Home Index with the login error
                return RedirectToAction("Index", "Home", new { loginError = true });
            }
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            // Clear the authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect to the home page after logout
            return RedirectToAction("Index", "Home");
        }

    }
}
