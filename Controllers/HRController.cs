using System;
using System.Linq;
using CMCSApplication.Data;
using CMCSApplication.Models;
using CMCSApplication.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCSApplication.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HRController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST ALL USERS
        
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

       
        // CREATE NEW USER
        
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            _context.Users.Add(user);
            _context.SaveChanges();

            // If user is Lecturer → auto create lecturer profile
            if (user.Role == "Lecturer")
            {
                var lecturer = new Lecturer
                {
                    Name = $"{user.Name} {user.Surname}",
                    Department = "Not Assigned",
                    HourlyRate = user.HourlyRate ?? 0,
                    Username = user.Username
                };

                _context.Lecturers.Add(lecturer);
                _context.SaveChanges();

                user.LecturerId = lecturer.Id;
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

       
        // EDIT USER
        
        public IActionResult Edit(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(User user)
        {
            if (!ModelState.IsValid)
                return View(user);

            _context.Users.Update(user);
            _context.SaveChanges();

            // Update linked lecturer info if needed
            if (user.Role == "Lecturer" && user.LecturerId != null)
            {
                var lec = _context.Lecturers.Find(user.LecturerId);
                if (lec != null)
                {
                    lec.Name = $"{user.Name} {user.Surname}";
                    lec.HourlyRate = user.HourlyRate ?? lec.HourlyRate;
                    _context.Lecturers.Update(lec);
                    _context.SaveChanges();
                }
            }

            return RedirectToAction("Index");
        }

        // USER DETAILS
        
        public IActionResult Details(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // DELETE USER
        
        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users.Find(id);

            if (user == null)
                return NotFound();

            // If linked to lecturer, remove lecturer profile too
            if (user.LecturerId != null)
            {
                var lecturer = _context.Lecturers.Find(user.LecturerId);
                if (lecturer != null)
                {
                    _context.Lecturers.Remove(lecturer);
                }
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // VIEW ALL CLAIMS
        
        public IActionResult ViewClaims()
        {
            var claims = _context.Claims.ToList();
            return View(claims);
        }

        // MODULE ASSIGNMENT (HR ONLY)

        // GET: HR/AssignModules
        public IActionResult AssignModules()
        {
            var vm = new ModuleAssignmentVM
            {
                Lecturers = _context.Lecturers.ToList(),
                Modules = _context.Modules.ToList(),
                Assignments = _context.ModuleAssignments
                    .Include(ma => ma.Lecturer)
                    .Include(ma => ma.Module)
                    .ToList()
            };

            return View(vm);
        }

        // POST: Assign a module to a lecturer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignModule(int lecturerId, int moduleId)
        {
            // Prevent duplicates
            if (!_context.ModuleAssignments.Any(ma => ma.LecturerId == lecturerId && ma.ModuleId == moduleId))
            {
                _context.ModuleAssignments.Add(new ModuleAssignment
                {
                    LecturerId = lecturerId,
                    ModuleId = moduleId
                });

                _context.SaveChanges();
                TempData["Success"] = "Module assigned successfully!";
            }
            else
            {
                TempData["Error"] = "This module is already assigned to that lecturer.";
            }

            return RedirectToAction("AssignModules");
        }

        // POST: Add module inline (from modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddModuleInline(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName))
            {
                TempData["Error"] = "Module name cannot be empty.";
                return RedirectToAction("AssignModules");
            }

            if (_context.Modules.Any(m => m.Name == moduleName))
            {
                TempData["Error"] = "This module already exists.";
                return RedirectToAction("AssignModules");
            }

            _context.Modules.Add(new Module { Name = moduleName });
            _context.SaveChanges();

            TempData["Success"] = "Module added successfully!";
            return RedirectToAction("AssignModules");
        }

        // POST: Delete a module assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAssignment(int id)
        {
            var assignment = _context.ModuleAssignments.FirstOrDefault(a => a.Id == id);

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
