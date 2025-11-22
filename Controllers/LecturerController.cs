using CMCSApplication.Data;
using CMCSApplication.Models;
using CMCSApplication.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClaimModel = CMCSApplication.Models.Claim;  

namespace CMCSApplication.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var lecturer = _context.Lecturers
                .Include(l => l.Department)
                .FirstOrDefault(l => l.Id == lecturerId);

            var recentClaims = _context.Claims
                .Where(c => c.LecturerId == lecturerId && !c.IsDeleted)
                .OrderByDescending(c => c.DateSubmitted)
                .Take(5)
                .ToList();

            var vm = new LecturerDashboardVM
            {
                Lecturer = lecturer,
                RecentClaims = recentClaims
            };

            return View(vm);
        }

        // View My Claims
        public IActionResult MyClaims()
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var claims = _context.Claims
                .Where(c => c.LecturerId == lecturerId && !c.IsDeleted)
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claims);
        }


        // GET: Submit a new claim
        [HttpGet]
        public IActionResult Submit()
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var lecturer = _context.Lecturers
                .Include(l => l.Department)
                .FirstOrDefault(l => l.Id == lecturerId);

            if (lecturer == null) return NotFound();

            // NEW: Block if lecturer has no department
            if (lecturer.Department == null)
            {
                TempData["Error"] = "You cannot submit a claim because your profile has no department assigned. Please contact HR to update your department.";
                return RedirectToAction(nameof(Index));
            }

            var model = new ClaimModel
            {
                LecturerId = lecturer.Id,
                LecturerName = lecturer.Name,
                HourlyRate = lecturer.HourlyRate,
                Department = lecturer.Department?.Name
            };

            return View(model);
        }

        // POST: Submit claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ClaimModel claim)
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var lecturer = _context.Lecturers
                .Include(l => l.Department)
                .First(l => l.Id == lecturerId);

            // NEW: Block if no department
            if (lecturer.Department == null)
            {
                TempData["Error"] = "Your profile is missing a department. Please contact HR to update your details before submitting claims.";
                return RedirectToAction(nameof(Index));
            }

            // Enforce server authority — ALWAYS override client values
            claim.LecturerId = lecturer.Id;
            claim.LecturerName = lecturer.Name;
            claim.Department = lecturer.Department.Name;  // now guaranteed not null
            claim.HourlyRate = lecturer.HourlyRate;
            claim.DateSubmitted = DateTime.Now;
            claim.Status = "Pending Verification";
            claim.Amount = claim.HoursWorked * lecturer.HourlyRate;

            // NEW: Validate model before trying to save
            if (!ModelState.IsValid)
            {
                return View(claim);
            }

            if (claim.HoursWorked > 220)
            {
                ModelState.AddModelError("HoursWorked", "You cannot claim more than 220 hours for a month.");
                return View(claim);
            }

            // File upload stays the same
            if (claim.UploadFile != null)
            {
                var ext = Path.GetExtension(claim.UploadFile.FileName).ToLower();
                var allowed = new[] { ".pdf", ".docx", ".xlsx" };

                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("UploadFile", "Only PDF, DOCX, XLSX allowed.");
                    return View(claim);
                }

                var folder = Path.Combine("wwwroot/uploads");
                Directory.CreateDirectory(folder);

                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(folder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    claim.UploadFile.CopyTo(stream);
                }

                claim.OriginalFileName = claim.UploadFile.FileName;
                claim.SupportingDocument = "/uploads/" + fileName;
            }

            _context.Claims.Add(claim);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Claim submitted successfully!";
            return RedirectToAction(nameof(MyClaims));
        }

        // Soft delete by lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClaim(int id)
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var claim = _context.Claims.FirstOrDefault(c => c.Id == id && c.LecturerId == lecturerId);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("MyClaims");
            }

            claim.IsDeleted = true;
            _context.Claims.Update(claim);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Claim deleted.";
            return RedirectToAction("MyClaims");
        }
    }
}
