using System;
using System.Linq;
using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
