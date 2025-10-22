using Microsoft.AspNetCore.Mvc;
using CMCSApplication.Data;
using CMCSApplication.Models;
using System.Linq;

namespace CMCSApplication.Controllers
{
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard
        public IActionResult Index()
        {
            return View();
        }

        // List of verified claims ready for manager approval
        public IActionResult Approval()
        {
            var verifiedClaims = _context.Claims
                .Where(c => c.Status == "Verified")
                .OrderByDescending(c => c.DateVerified)
                .ToList();

            return View(verifiedClaims);
        }

        // Review a single verified claim
        public IActionResult Review(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            claim.Status = "Approved by Manager";
            claim.DateApproved = DateTime.Now;

            _context.Update(claim);
            _context.SaveChanges();

            TempData["Success"] = "Claim approved successfully!";
            return RedirectToAction("Approval");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("ApprovalQueue");
            }

            claim.Status = "Rejected by Manager";
            claim.DateApproved = DateTime.Now;

            _context.Update(claim);
            _context.SaveChanges();

            TempData["Info"] = "Claim rejected.";
            return RedirectToAction("Approval");
        }

        // Reports page (you already have)
        public IActionResult Reports()
        {
            return View();
        }

        // Assign coordinators page (you already have)
        public IActionResult Assign()
        {
            return View();
        }
    }
}
