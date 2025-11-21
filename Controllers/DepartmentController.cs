using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CMCSApplication.Controllers
{
    [Authorize(Roles = "HR")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Department
        public IActionResult Index()
        {
            var departments = _context.Departments
                .OrderBy(d => d.Name)
                .ToList();

            return View(departments);
        }

        // GET: /Department/Details/5
        public IActionResult Details(int id)
        {
            var department = _context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        // GET: /Department/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Department/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Department department)
        {
            if (!ModelState.IsValid)
                return View(department);

            _context.Departments.Add(department);
            _context.SaveChanges();

            TempData["Success"] = "Department created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Department/Edit/5
        public IActionResult Edit(int id)
        {
            var department = _context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: /Department/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Department department)
        {
            if (!ModelState.IsValid)
                return View(department);

            _context.Departments.Update(department);
            _context.SaveChanges();

            TempData["Success"] = "Department updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Department/Delete/5
        public IActionResult Delete(int id)
        {
            var department = _context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null) return NotFound();

            return View(department);
        }

        // POST: /Department/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var department = _context.Departments.FirstOrDefault(d => d.Id == id);
            if (department == null) return NotFound();

            _context.Departments.Remove(department);
            _context.SaveChanges();

            TempData["Success"] = "Department deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
