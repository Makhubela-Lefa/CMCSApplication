using CMCSApplication.Data;
using CMCSApplication.Models;
using CMCSApplication.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClaimModel = CMCSApplication.Models.Claim;   // FIXES AMBIGUOUS CLAIM ERROR

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
                .Include(l => l.DepartmentRef)
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

            var lecturer = _context.Lecturers.FirstOrDefault(l => l.Id == lecturerId);
            if (lecturer == null) return NotFound();

            var model = new ClaimModel
            {
                LecturerId = lecturer.Id,
                LecturerName = lecturer.Name,
                HourlyRate = lecturer.HourlyRate
            };

            return View(model);
        }


        // POST: Submit claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(ClaimModel claim)
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var lecturer = _context.Lecturers.First(l => l.Id == lecturerId);

            // Override fields so lecturer cannot tamper
            claim.LecturerId = lecturer.Id;
            claim.LecturerName = lecturer.Name;
            claim.HourlyRate = lecturer.HourlyRate;
            claim.Status = "Pending Verification";
            claim.DateSubmitted = DateTime.Now;
            claim.Amount = claim.HoursWorked * lecturer.HourlyRate;

            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "You cannot claim more than 180 hours.");
                return View(claim);
            }

            // Upload document
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


        // GET: Upload Page (only own claims)
        [HttpGet]
        public IActionResult Upload()
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var claims = _context.Claims
                .Where(c => c.LecturerId == lecturerId && !c.IsDeleted)
                .ToList();

            ViewBag.Claims = claims;

            return View();
        }


        // POST: Upload file for a specific claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(int claimId, IFormFile file)
        {
            int lecturerId = int.Parse(User.Claims.First(c => c.Type == "LecturerId").Value);

            var claim = _context.Claims
                .FirstOrDefault(c => c.Id == claimId && c.LecturerId == lecturerId);

            if (claim == null)
            {
                TempData["UploadMessage"] = "Invalid claim.";
                return RedirectToAction(nameof(Upload));
            }

            if (file == null || file.Length == 0)
            {
                TempData["UploadMessage"] = "Please select a valid file.";
                return RedirectToAction(nameof(Upload));
            }

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".pdf", ".docx", ".xlsx" };
            if (!allowed.Contains(ext))
            {
                TempData["UploadMessage"] = "Invalid file type.";
                return RedirectToAction(nameof(Upload));
            }

            var folder = Path.Combine("wwwroot/uploads");
            Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + ext;
            var path = Path.Combine(folder, fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                file.CopyTo(stream);
            }

            claim.SupportingDocument = "/uploads/" + fileName;
            claim.OriginalFileName = file.FileName;

            _context.Update(claim);
            _context.SaveChanges();

            TempData["UploadMessage"] = "Uploaded successfully!";
            return RedirectToAction(nameof(Upload));
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
