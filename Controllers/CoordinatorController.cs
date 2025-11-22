using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCSApplication.Controllers
{
    [Authorize(Roles = "Coordinator")]
    public class CoordinatorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CoordinatorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public IActionResult Index()
        {
            var pendingCount = _context.Claims.Count(c => c.Status == "Pending Verification");
            var verifiedCount = _context.Claims.Count(c => c.Status == "Verified by Coordinator");
            var rejectedCount = _context.Claims.Count(c => c.Status == "Rejected by Coordinator");

            ViewBag.PendingCount = pendingCount;
            ViewBag.VerifiedCount = verifiedCount;
            ViewBag.RejectedCount = rejectedCount;

            return View();
        }

        // VIEW PENDING CLAIMS
        public IActionResult VerifyQueue()
        {
            var pendingClaims = _context.Claims
                .Where(c => c.Status == "Pending Verification")
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(pendingClaims);
        }


        // APPROVE A CLAIM
        [HttpPost]
        public IActionResult Approve(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(VerifyQueue));
            }

            string coordinatorUsername = User.Identity!.Name!;

            claim.Status = "Verified by Coordinator";
            claim.CoordinatorStatus = "Approved";
            claim.ManagerStatus = "Pending Approval";
            claim.DateVerified = DateTime.Now;
            claim.CoordinatorId = coordinatorUsername;

            _context.Update(claim);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Claim verified successfully!";
            return RedirectToAction(nameof(VerifyQueue));
        }

        // REJECT CLAIM
        [HttpPost]
        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["ErrorMessage"] = "Claim not found.";
                return RedirectToAction(nameof(VerifyQueue));
            }

            string coordinatorUsername = User.Identity!.Name!;

            claim.CoordinatorStatus = "Rejected";
            claim.Status = "Rejected by Coordinator";
            claim.ManagerStatus = "N/A";
            claim.DateVerified = DateTime.Now;
            claim.CoordinatorId = coordinatorUsername;

            _context.Update(claim);
            _context.SaveChanges();

            TempData["ErrorMessage"] = "Claim rejected.";
            return RedirectToAction(nameof(VerifyQueue));
        }


        // REVIEW QUEUE (view all: pending, verified, rejected)
        public IActionResult ReviewQueue()
        {
            var claims = _context.Claims
                .Where(c =>
                    c.Status == "Pending Verification" ||
                    c.Status == "Verified by Coordinator" ||
                    c.Status == "Rejected by Coordinator")
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claims);
        }
    }
}
