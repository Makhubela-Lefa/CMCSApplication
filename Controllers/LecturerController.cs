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
            return View();
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

        // Get to Submit the Claim Form
        [HttpGet]
        public IActionResult Submit()
        {
            return View();
        }

        // Post to Submit the Claim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Submit(Claim claim, IFormFile file)
        {
            if (ModelState.IsValid)
            {
                // Handle file upload
                if (file != null && file.Length > 0)
                {
                    var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx" };
                    var ext = Path.GetExtension(file.FileName).ToLower();

                    if (!allowedExtensions.Contains(ext))
                    {
                        TempData["UploadMessage"] = "Invalid file type! Only PDF, DOCX, XLSX are allowed.";
                        return View(claim);
                    }

                    if (file.Length > 5 * 1024 * 1024) // 5 MB limit
                    {
                        TempData["UploadMessage"] = "File too large! Maximum allowed size is 5 MB.";
                        return View(claim);
                    }

                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = Guid.NewGuid() + ext; // Unique file name
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    claim.OriginalFileName = file.FileName;
                    claim.SupportingDocument = fileName;
                }

                // Set claim metadata
                claim.Status = "Pending Verification";
                claim.DateSubmitted = DateTime.Now;

                _context.Claims.Add(claim);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(MyClaims));
            }

            return View(claim);
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
