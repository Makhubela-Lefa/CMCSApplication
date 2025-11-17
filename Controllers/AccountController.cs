using CMCSApplication.Data;
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
        public IActionResult Login(int lecturerId)
        {
            var lecturer = _context.Lecturers.FirstOrDefault(l => l.Id == lecturerId);
            if (lecturer == null)
            {
                TempData["Error"] = "Lecturer not found";
                return RedirectToAction(nameof(Login));
            }

            HttpContext.Session.SetInt32("LecturerId", lecturer.Id);
            HttpContext.Session.SetString("LecturerName", lecturer.Name);

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
