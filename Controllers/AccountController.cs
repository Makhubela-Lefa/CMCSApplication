using System.Security.Claims;
using CMCSApplication.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMCSApplication.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AccountController(ApplicationDbContext context) => _context = context;

        // GET: /Account/Login
        public IActionResult Login()
        {
            var lecturers = _context.Lecturers.OrderBy(l => l.Name).ToList();
            return View(lecturers);
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(int lecturerId)
        {
            var lecturer = _context.Lecturers.FirstOrDefault(l => l.Id == lecturerId);
            if (lecturer == null)
            {
                TempData["Error"] = "Lecturer not found";
                return RedirectToAction(nameof(Login));
            }

            // --- Session (still allowed to keep for convenience) ---
            HttpContext.Session.SetInt32("LecturerId", lecturer.Id);
            HttpContext.Session.SetString("LecturerName", lecturer.Name);

            // --- Cookie Authentication (required for [Authorize]) ---
            var claims = new[]
            {
        new Claim("LecturerId", lecturer.Id.ToString()),
        new Claim(ClaimTypes.Name, lecturer.Name)
    };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            TempData["SuccessMessage"] = $"Logged in as {lecturer.Name}";
            return RedirectToAction("Index", "Lecturer");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Remove("LecturerId");
            HttpContext.Session.Remove("LecturerName");
            TempData["SuccessMessage"] = "Logged out";
            return RedirectToAction("Login");
        }
    }
}
