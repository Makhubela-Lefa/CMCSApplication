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
                .Where(c => c.Status == "Verified" || c.Status.Contains("Pending"))
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

        // Reports page
        public IActionResult Reports()
        {
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

    }
}