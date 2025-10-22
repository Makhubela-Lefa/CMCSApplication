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
            var claims = _context.Claims.ToList();
            return View(claims);
        }

        // GET: Lecturer/Submit
        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        // POST: Lecturer/Submit 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(Claim claim)
        {
            // --- Debug: log model validation errors ---
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            if (!ModelState.IsValid)
            {
                TempData["UploadMessage"] = "Please correct the errors below and try again.";
                return View(claim);
            }

            try
            {
                // --- File Upload Handling ---
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

                // --- NEW: Link claim to Lecturer ---
                var existingLecturer = _context.Lecturers
                    .FirstOrDefault(l => l.Name == claim.LecturerName);

                if (existingLecturer == null)
                {
                    existingLecturer = new Lecturer
                    {
                        Name = claim.LecturerName,
                        Department = claim.Department
                    };

                    _context.Lecturers.Add(existingLecturer);
                    _context.SaveChanges();
                }

                claim.LecturerId = existingLecturer.Id; // establish link

                // --- Claim Metadata ---
                claim.Status = "Pending Verification";
                claim.DateSubmitted = DateTime.Now;

                _context.Claims.Add(claim);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "✅ Claim submitted successfully!";
                return RedirectToAction(nameof(MyClaims));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while submitting the claim: {ex.Message}");
                return View(claim);
            }
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
    }
}
