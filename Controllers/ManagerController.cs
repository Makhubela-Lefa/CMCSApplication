using System.Linq;
using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            // Get the 5 most recent claims for display
            ViewBag.RecentClaims = _context.Claims
                .Include(c => c.Module)
                .OrderByDescending(c => c.DateSubmitted)
                .Take(5)
                .ToList();

            return View();
        }

        public IActionResult Approval()
        {
            // Get claims that are either Verified by Coordinator or Pending
            var claimsForManager = _context.Claims
               .Where(c => c.Status == "Verified by Coordinator")
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claimsForManager);
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
            claim.ManagerStatus = "Approved";
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
                return RedirectToAction("Approval");
            }

            claim.Status = "Rejected by Manager";
            claim.ManagerStatus = "Rejected";
            claim.DateApproved = DateTime.Now;

            _context.Update(claim);
            _context.SaveChanges();

            TempData["Info"] = "Claim rejected.";
            return RedirectToAction("Approval");
        }

        // Reports page
        public IActionResult Reports()
        {
            // Get all claims
            var claims = _context.Claims.ToList();

            // Total claims
            ViewBag.TotalClaims = claims.Count;

            // Total payout (sum of all TotalAmount)
            ViewBag.TotalPayout = claims.Sum(c => c.TotalAmount);

            // Department breakdown
            ViewBag.DepartmentTotals = claims
                .GroupBy(c => c.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    TotalAmount = g.Sum(c => c.TotalAmount)
                })
                .ToList();

            return View();
        }

        // GET: Assign Modules
        public IActionResult AssignModules()
        {
            ViewBag.Lecturers = _context.Lecturers.ToList();
            ViewBag.Modules = _context.Modules.ToList();
            ViewBag.Assignments = _context.ModuleAssignments
                                          .Include(ma => ma.Lecturer)
                                          .Include(ma => ma.Module)
                                          .ToList();
            return View();
        }

        // POST: Assign Module
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignModule(int lecturerId, int moduleId)
        {
            if (!_context.ModuleAssignments.Any(ma => ma.LecturerId == lecturerId && ma.ModuleId == moduleId))
            {
                var assignment = new ModuleAssignment
                {
                    LecturerId = lecturerId,
                    ModuleId = moduleId
                };
                _context.ModuleAssignments.Add(assignment);
                _context.SaveChanges();
                TempData["Success"] = "Module assigned successfully!";
            }
            else
            {
                TempData["Error"] = "This module is already assigned to the lecturer.";
            }
            return RedirectToAction("AssignModules");
        }

        // POST: Add Module Inline (from modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddModuleInline(string moduleName)
        {
            if (!string.IsNullOrWhiteSpace(moduleName))
            {
                if (!_context.Modules.Any(m => m.Name == moduleName))
                {
                    var module = new Module { Name = moduleName };
                    _context.Modules.Add(module);
                    _context.SaveChanges();
                    TempData["Success"] = "Module added successfully!";
                }
                else
                {
                    TempData["Error"] = "This module already exists.";
                }
            }
            else
            {
                TempData["Error"] = "Module name cannot be empty.";
            }

            return RedirectToAction("AssignModules");
        }

        // POST: Delete Assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAssignment(int id)
        {
            var assignment = _context.ModuleAssignments.FirstOrDefault(ma => ma.Id == id);
            if (assignment != null)
            {
                _context.ModuleAssignments.Remove(assignment);
                _context.SaveChanges();
                TempData["Success"] = "Assignment deleted successfully!";
            }
            else
            {
                TempData["Error"] = "Assignment not found.";
            }
            return RedirectToAction("AssignModules");
        }

        // GET: Manager/EditClaim/5
        public IActionResult EditClaim(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            ViewBag.Lecturers = _context.Lecturers.ToList();
            ViewBag.Modules = _context.Modules.ToList();

            return View(claim);
        }

        // POST: Manager/EditClaim/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClaim(Claim claim)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Lecturers = _context.Lecturers.ToList();
                ViewBag.Modules = _context.Modules.ToList();
                return View(claim);
            }

            var existingClaim = _context.Claims.FirstOrDefault(c => c.Id == claim.Id);
            if (existingClaim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            // Update fields
            existingClaim.LecturerId = claim.LecturerId;
            existingClaim.LecturerName = claim.LecturerName;
            existingClaim.Department = claim.Department;
            existingClaim.Month = claim.Month;
            existingClaim.HoursWorked = claim.HoursWorked;
            existingClaim.HourlyRate = claim.HourlyRate;
            existingClaim.Notes = claim.Notes;
            existingClaim.ModuleId = claim.ModuleId;

            _context.SaveChanges();
            TempData["Success"] = "Claim updated successfully!";
            return RedirectToAction("Approval");
        }

        // POST: Manager/DeleteClaim/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClaim(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            _context.Claims.Remove(claim);
            _context.SaveChanges();
            TempData["Success"] = "Claim deleted successfully!";
            return RedirectToAction("Approval");
        }
        
        // POST: Manager/DeleteClaimFromIndex
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClaimFromIndex(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id);
            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Index");
            }

            _context.Claims.Remove(claim);
            _context.SaveChanges();

            TempData["Success"] = "Claim deleted successfully!";
            return RedirectToAction("Index");
        }

    }
}