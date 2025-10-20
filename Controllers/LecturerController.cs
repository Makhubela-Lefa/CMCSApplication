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
                // Handle file upload if a file was selected
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

                    var fileName = Guid.NewGuid() + ext; // Unique file name to prevent collisions
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }

                    // Save uploaded file name in the claim
                    claim.SupportingDocument = fileName;
                }

                // Set claim status and date
                claim.Status = "Pending Verification";
                claim.DateSubmitted = DateTime.Now;

                // Save claim to database
                _context.Claims.Add(claim);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(MyClaims));
            }

            return View(claim);
        }


        // Upload Supporting Document
        [HttpPost]
        public IActionResult Upload(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var filePath = Path.Combine(uploadsFolder, file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(stream);
                }

                TempData["UploadMessage"] = "File uploaded successfully!";
            }
            else
            {
                TempData["UploadMessage"] = "Please select a valid file to upload.";
            }

            return RedirectToAction(nameof(Submit));
        }
    }
}
