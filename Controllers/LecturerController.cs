using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCSApplication.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lecturer Home
        public IActionResult Index()
        {
            var recentClaims = _context.Claims
                .OrderByDescending(c => c.DateSubmitted)
                .Take(5)
                .ToList();

            return View(recentClaims);
        }

        // Lecturer Profile
        public IActionResult Profile()
        {
            return View();
        }

        // View All Submitted Claims
        public IActionResult MyClaims()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // get lecturer profile
            var lecturer = _context.Lecturers.FirstOrDefault(l => l.Username == username);
            if (lecturer == null)
                return NotFound("Lecturer profile not found.");

            // only fetch claims that belong to this lecturer
            var claims = _context.Claims
                .Where(c => c.LecturerId == lecturer.Id)
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claims);
        }

        // GET: Lecturer/Submit
        [HttpGet]
        public IActionResult Submit()
        {
            // ❗ Lecturer must be logged in
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // ❗ Pull lecturer info
            var lecturer = _context.Lecturers.FirstOrDefault(l => l.Username == username);
            if (lecturer == null)
                return NotFound("Lecturer profile not found.");

            // Prefill model (rate + personal info)
            var model = new Claim
            {
                LecturerId = lecturer.Id,
                LecturerName = lecturer.Name,
                HourlyRate = lecturer.HourlyRate
            };

            return View(model);
        }

        // POST: Lecturer/Submit 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(Claim claim)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            var lecturer = _context.Lecturers.FirstOrDefault(l => l.Username == username);
            if (lecturer == null)
                return NotFound("Lecturer profile not found.");

            // Override sensitive fields (cannot come from form)
            claim.LecturerId = lecturer.Id;
            claim.LecturerName = lecturer.Name;
            claim.HourlyRate = lecturer.HourlyRate;

            // Validation: maximum is 180 hours
            if (claim.HoursWorked > 180)
            {
                ModelState.AddModelError("HoursWorked", "You cannot claim more than 180 hours in one month.");
                return View(claim);
            }

            //Auto-calculation
            claim.Amount = claim.HoursWorked * claim.HourlyRate;

            //file upload logic
            if (claim.UploadFile != null && claim.UploadFile.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                var ext = Path.GetExtension(claim.UploadFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("UploadFile", "Invalid file type! Only PDF, DOCX, XLSX are allowed.");
                    return View(claim);
                }

                if (claim.UploadFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("UploadFile", "File too large! Maximum allowed size is 5 MB.");
                    return View(claim);
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    claim.UploadFile.CopyTo(stream);
                }

                claim.OriginalFileName = claim.UploadFile.FileName;
                claim.SupportingDocument = $"/uploads/{uniqueFileName}";
            }

            // --- Claim Metadata ---
            claim.Status = "Pending Verification";
            claim.DateSubmitted = DateTime.Now;

            _context.Claims.Add(claim);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Claim submitted successfully!";
            return RedirectToAction(nameof(MyClaims));
        }

        // GET: Upload Documents Page
        [HttpGet]
        public IActionResult Upload()
        {
            var claims = _context.Claims.ToList();
            ViewBag.Claims = claims;
            return View();
        }

        // POST: Handle file upload for a specific claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upload(int claimId, IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                var ext = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(ext))
                {
                    TempData["UploadMessage"] = "Invalid file type! Only PDF, DOCX, XLSX allowed.";
                    return RedirectToAction(nameof(Upload));
                }

                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                var claim = _context.Claims.FirstOrDefault(c => c.Id == claimId);
                if (claim != null)
                {
                    claim.SupportingDocument = fileName;
                    claim.OriginalFileName = file.FileName;
                    _context.Update(claim);
                    _context.SaveChanges();

                    TempData["UploadMessage"] = "File uploaded successfully!";
                }
                else
                {
                    TempData["UploadMessage"] = "Claim not found.";
                }
            }
            else
            {
                TempData["UploadMessage"] = "Please select a valid file to upload.";
            }

            return RedirectToAction(nameof(Upload));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClaim(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("MyClaims");
            }

            _context.Claims.Remove(claim);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Claim deleted successfully!";
            return RedirectToAction("MyClaims");
        }


    }
}
