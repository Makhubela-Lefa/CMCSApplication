using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var pendingCount = _context.Claims.Count(c => c.Status == "Pending Verification");
            var verifiedCount = _context.Claims.Count(c => c.Status == "Verified");
            var rejectedCount = _context.Claims.Count(c => c.Status == "Rejected by Coordinator");

            ViewBag.PendingCount = pendingCount;
            ViewBag.VerifiedCount = verifiedCount;
            ViewBag.RejectedCount = rejectedCount;

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

        [HttpPost]
        public IActionResult Approve(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Verified by Coordinator";
                claim.CoordinatorStatus = "Verified";
                claim.DateVerified = DateTime.Now;
                claim.CoordinatorId = User.Identity?.Name;


                _context.Update(claim);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Claim approved and ready for manager!";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction(nameof(VerifyQueue));
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Rejected by Coordinator";
                claim.CoordinatorStatus = "Rejected";
                claim.DateVerified = DateTime.Now;
                claim.CoordinatorId = User.Identity?.Name;


                _context.Update(claim);
                _context.SaveChanges();

                TempData["ErrorMessage"] = "Claim rejected by coordinator.";
            }
            else
            {
                TempData["ErrorMessage"] = "Claim not found.";
            }

            return RedirectToAction(nameof(VerifyQueue));
        }
        // View all claims for review (simplified, no filters)
        public IActionResult ReviewQueue()
        {
            // Get all pending claims
            var claims = _context.Claims
                .Where(c => c.Status == "Pending Verification" || c.Status == "Verified by Coordinator" || c.Status == "Rejected by Coordinator")
                .ToList();

            return View(claims);
        }

    }
}
