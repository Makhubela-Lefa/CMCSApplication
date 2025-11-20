using System.Security.Claims;
using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace CMCSApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context) => _context = context;

        // LOGIN (GET)
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                TempData["Error"] = "Enter both username and password.";
                return View();
            }

            var user = _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);

            if (user == null)
            {
                TempData["Error"] = "Invalid login credentials.";
                return View();
            }

            // Save session (optional but useful for quick display)
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetInt32("UserId", user.Id);

            // Create cookie authentication claims
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Name, user.Username),
                new System.Security.Claims.Claim(ClaimTypes.Role, user.Role),
                new System.Security.Claims.Claim("UserId", user.Id.ToString())
            };

            // If user is a lecturer, store LecturerId for claims
            if (user.Role == "Lecturer" && user.LecturerId.HasValue)
            {
                claims.Add(new System.Security.Claims.Claim("LecturerId", user.LecturerId.Value.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            TempData["SuccessMessage"] = $"Welcome {user.Username}";

            // Role-based redirect
            return user.Role switch
            {
                "HR" => RedirectToAction("Index", "HR"),
                "Manager" => RedirectToAction("Index", "Manager"),
                "Coordinator" => RedirectToAction("VerifyQueue", "Coordinator"),
                "Lecturer" => RedirectToAction("Index", "Lecturer"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // LOGOUT
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Logged out successfully.";
            return RedirectToAction(nameof(Login));
        }
    }
}
