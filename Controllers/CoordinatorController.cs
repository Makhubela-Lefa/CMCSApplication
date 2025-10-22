using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCSApplication.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Coordinator Home
        public IActionResult Index()
        {
            return View();
        }

        // View all Pending Claims for Verification
        public IActionResult VerifyQueue()
        {
            var pendingClaims = _context.Claims
                .Where(c => c.Status == "Pending Verification")
                .ToList();

            return View(pendingClaims);
        }

        // Approve a Claim
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.DateVerified = DateTime.Now; // record the date it was verified

                _context.Update(claim); // tells EF Core that the entity has changed
                _context.SaveChanges(); // actually saves it to the database

                TempData["SuccessMessage"] = "Claim approved successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction(nameof(VerifyQueue));
        }



        // Reject a Claim
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.DateVerified = DateTime.Now; // record rejection date

                _context.Update(claim);
                _context.SaveChanges();

                TempData["ErrorMessage"] = "Claim rejected.";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction(nameof(VerifyQueue));
        }


        // Optional: Detailed Review View for Individual Claim
        // Review a specific claim in detail
        public IActionResult Review(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);

            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(VerifyQueue));
            }

            return View(claim);
        }

    }
}
