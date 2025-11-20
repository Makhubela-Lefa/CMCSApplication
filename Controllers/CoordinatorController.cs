using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCSApplication.Controllers
{
    [Authorize]
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
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

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
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            var pendingClaims = _context.Claims
                .Where(c => c.Status == "Pending Verification")
                .ToList();

            return View(pendingClaims);
        }


        [HttpPost]
        public IActionResult Approve(int id)
        {
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim != null)
            {
                claim.Status = "Verified by Coordinator";
                claim.CoordinatorStatus = "Verified";
                claim.DateVerified = DateTime.Now;
                claim.CoordinatorId = username;


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
            // 1. Ensure coordinator is logged in
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // 2. Retrieve the claim
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(VerifyQueue));
            }

            // 3. Update claim details
            claim.Status = "Rejected by Coordinator";
            claim.CoordinatorStatus = "Rejected";
            claim.DateVerified = DateTime.Now;
            claim.CoordinatorId = username; // Store coordinator username

            _context.Update(claim);
            _context.SaveChanges();

            // 4. Feedback
            TempData["ErrorMessage"] = "Claim rejected by coordinator.";

            return RedirectToAction(nameof(VerifyQueue));
        }

        // View all claims for review (simplified, no filters)
        public IActionResult ReviewQueue()
        {
            // Ensure coordinator is logged in
            var username = HttpContext.Session.GetString("Username");
            if (username == null)
                return RedirectToAction("Login", "Account");

            // Fetch all claims relevant to coordinator review
            var claims = _context.Claims
                .Where(c =>
                    c.Status == "Pending Verification" ||
                    c.Status == "Verified by Coordinator" ||
                    c.Status == "Rejected by Coordinator")
                .ToList();

            return View(claims);
        }


    }
}
