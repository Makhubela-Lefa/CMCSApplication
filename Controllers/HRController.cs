using System;
using System.Linq;
using CMCSApplication.Data;
using CMCSApplication.Models;
using CMCSApplication.Models.ViewModels;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

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
            var users = _context.Users
                .Include(u => u.Department)
                .ToList();

            return View(users);
        }


        // CREATE NEW USER

        // GET: Create User
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }


        // POST: Create User
        [HttpPost]
        public IActionResult Create(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return View(user);
            }

            // If Lecturer → apply department hourly rate
            if (user.Role == "Lecturer" && user.DepartmentId.HasValue)
            {
                var dept = _context.Departments.FirstOrDefault(d => d.Id == user.DepartmentId);
                if (dept != null)
                {
                    user.HourlyRate = dept.HourlyRate;
                }
            }
            else
            {
                // Non-lecturers do not have a department or hourly rate
                user.DepartmentId = null;
                user.HourlyRate = null;
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            // Auto create Lecturer profile ONLY if Lecturer
            if (user.Role == "Lecturer")
            {
                var lecturer = new Lecturer
                {
                    Name = $"{user.Name} {user.Surname}",
                    Username = user.Username,
                    DepartmentId = user.DepartmentId,
                    HourlyRate = user.HourlyRate ?? 0
                };

                _context.Lecturers.Add(lecturer);
                _context.SaveChanges();

                // Link the lecturer profile to the user account
                user.LecturerId = lecturer.Id;
                _context.Users.Update(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }


        // EDIT USER
        // GET: Edit User
        public IActionResult Edit(int id)
        {
            var user = _context.Users
                .Include(u => u.Department)
                .Include(u => u.Lecturer)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            ViewBag.Departments = _context.Departments.ToList();
            return View(user);
        }


        // POST: Edit User
        [HttpPost]
        public IActionResult Edit(User user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ViewBag.Departments = _context.Departments.ToList();
                    return View(user);
                }

                var existing = _context.Users
                    .Include(u => u.Lecturer)
                    .FirstOrDefault(u => u.Id == user.Id);

                if (existing == null)
                    return NotFound();

                // Update basic fields
                existing.Name = user.Name;
                existing.Surname = user.Surname;
                existing.Email = user.Email;
                existing.Role = user.Role;
                existing.Username = user.Username;

                // -------------------------------
                // CASE 1 — USER IS A LECTURER
                // -------------------------------
                if (user.Role == "Lecturer")
                {
                    // UPDATE DEPARTMENT ID FIRST
                    existing.DepartmentId = user.DepartmentId;

                    // APPLY HOURLY RATE FROM DEPARTMENT
                    if (user.DepartmentId.HasValue)
                    {
                        var dept = _context.Departments.FirstOrDefault(d => d.Id == user.DepartmentId);
                        if (dept != null)
                            existing.HourlyRate = dept.HourlyRate;
                    }

                    // CREATE lecturer record if missing
                    if (existing.LecturerId == null)
                    {
                        var newLecturer = new Lecturer
                        {
                            Name = $"{existing.Name} {existing.Surname}",
                            Username = existing.Username,
                            DepartmentId = existing.DepartmentId,
                            HourlyRate = existing.HourlyRate ?? 0
                        };

                        _context.Lecturers.Add(newLecturer);
                        _context.SaveChanges();

                        existing.LecturerId = newLecturer.Id;
                    }
                    else
                    {
                        // UPDATE LINKED LECTURER
                        var lec = existing.Lecturer;

                        lec.Name = $"{existing.Name} {existing.Surname}";
                        lec.Username = existing.Username;
                        lec.DepartmentId = existing.DepartmentId;
                        lec.HourlyRate = existing.HourlyRate ?? lec.HourlyRate;

                        _context.Lecturers.Update(lec);
                    }
                }
                else
                {
                    // -------------------------------
                    // CASE 2 — NOT A LECTURER
                    // -------------------------------
                    existing.DepartmentId = null;
                    existing.HourlyRate = null;

                    if (existing.LecturerId != null)
                    {
                        var lec = _context.Lecturers.FirstOrDefault(l => l.Id == existing.LecturerId);
                        if (lec != null)
                            _context.Lecturers.Remove(lec);

                        existing.LecturerId = null;
                    }
                }

                _context.Users.Update(existing);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // TEMP DEBUG: show exact error message
                TempData["Error"] = "ERROR: " + ex.Message;
                return RedirectToAction("Edit", new { id = user.Id });
            }
        }

        // USER DETAILS
        public IActionResult Details(int id)
        {
            var user = _context.Users
                .Include(u => u.Department)
                .Include(u => u.Lecturer)
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

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

        // MANAGE LECTURERS
        public IActionResult ManageLecturers()
        {
            var lecturers = _context.Lecturers
                .Include(l => l.Department)
                .Select(l => new LecturerWithUserIdVM
                {
                    Lecturer = l,
                    UserId = _context.Users
                             .Where(u => u.LecturerId == l.Id)
                             .Select(u => u.Id)
                             .FirstOrDefault()
                })
                .ToList();

            return View(lecturers);
        }


        // EXPORT ALL CLAIMS AS PDF (HR)
        public IActionResult ExportClaimsPdf()
        {
            var claims = _context.Claims
                .Include(c => c.Lecturer)
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            using var ms = new MemoryStream();
            var document = new iTextSharp.text.Document();
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms);

            document.Open();

            // GREEN TITLE 
            var titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(34, 139, 34));
            document.Add(new iTextSharp.text.Paragraph("CMCS – Full Claims Report", titleFont));
            document.Add(new iTextSharp.text.Paragraph("\n"));

            // SUMMARY
            var summaryFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL);

            document.Add(new iTextSharp.text.Paragraph($"Total Claims: {claims.Count}", summaryFont));
            document.Add(new iTextSharp.text.Paragraph($"Total Amount: R {claims.Sum(c => c.TotalAmount):N2}", summaryFont));
            document.Add(new iTextSharp.text.Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}", summaryFont));
            document.Add(new iTextSharp.text.Paragraph("\n"));

            // GREEN TABLE HEADER
            var table = new iTextSharp.text.pdf.PdfPTable(7) { WidthPercentage = 100 };

            var headerFont = iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD, new iTextSharp.text.BaseColor(34, 139, 34));

            string[] headers = { "Lecturer", "Department", "Month", "Hours", "Rate", "Amount", "Status" };

            foreach (var h in headers)
            {
                var cell = new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase(h, headerFont))
                {
                    BackgroundColor = new iTextSharp.text.BaseColor(232, 255, 232) // light green
                };
                table.AddCell(cell);
            }

            // ====== TABLE BODY ======
            foreach (var c in claims)
            {
                table.AddCell(c.LecturerName);
                table.AddCell(c.Department);
                table.AddCell(c.Month);
                table.AddCell(c.HoursWorked.ToString());
                table.AddCell("R " + c.HourlyRate.ToString("N2"));
                table.AddCell("R " + c.TotalAmount.ToString("N2"));
                table.AddCell(c.Status);
            }

            document.Add(table);
            document.Close();

            return File(ms.ToArray(), "application/pdf", "HR_AllClaimsReport.pdf");
        }

        // HR: Select Lecturer to Generate Invoice
        public IActionResult Invoice()
        {
            var lecturers = _context.Lecturers.ToList();
            return View(lecturers);
        }

[HttpPost]
    public IActionResult GenerateInvoice(int lecturerId)
    {
        var lecturer = _context.Lecturers.FirstOrDefault(l => l.Id == lecturerId);
        if (lecturer == null)
            return NotFound();

        var claims = _context.Claims
            .Where(c => c.LecturerId == lecturerId && c.ManagerStatus == "Approved")
            .OrderByDescending(c => c.Month)
            .ToList();

        if (!claims.Any())
        {
            TempData["Error"] = "No approved claims for this lecturer.";
            return RedirectToAction("Invoice");
        }

        using var ms = new MemoryStream();
        var doc = new Document(PageSize.A4, 30, 30, 30, 30);
        PdfWriter.GetInstance(doc, ms);

        doc.Open();

        // Green header color
        var green = new BaseColor(15, 140, 90);

        var titleFont = FontFactory.GetFont("Arial", 18, Font.BOLD, green);
        var headerFont = FontFactory.GetFont("Arial", 12, Font.BOLD);
        var normalFont = FontFactory.GetFont("Arial", 10);

        // Title
        doc.Add(new Paragraph("LECTURER INVOICE", titleFont));
        doc.Add(new Paragraph("\n"));

        // Lecturer info
        doc.Add(new Paragraph($"Name: {lecturer.Name}", normalFont));
            doc.Add(new Paragraph($"Department: {lecturer.Department?.Name ?? "Not Assigned"}", normalFont));
            doc.Add(new Paragraph($"Hourly Rate: R {lecturer.HourlyRate}", normalFont));
        doc.Add(new Paragraph("\n"));

        // Table
        var table = new PdfPTable(4) { WidthPercentage = 100 };
        table.SetWidths(new float[] { 2, 1, 1, 1 });

        // Table headers
        void AddHeader(string text)
        {
            var cell = new PdfPCell(new Phrase(text, headerFont))
            {
                BackgroundColor = green,
                Padding = 5,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            table.AddCell(cell);
        }

        AddHeader("Month");
        AddHeader("Hours Worked");
        AddHeader("Rate");
        AddHeader("Total");

        decimal grandTotal = 0;

        foreach (var c in claims)
        {
            table.AddCell(new Phrase(c.Month, normalFont));
            table.AddCell(new Phrase(c.HoursWorked.ToString(), normalFont));
            table.AddCell(new Phrase($"R {c.HourlyRate}", normalFont));

            var total = c.HoursWorked * c.HourlyRate;
            grandTotal += total;

            table.AddCell(new Phrase($"R {total}", normalFont));
        }

        doc.Add(table);
        doc.Add(new Paragraph("\n"));

        // Grand Total
        var totalParagraph = new Paragraph($"Grand Total: R {grandTotal}", titleFont)
        {
            Alignment = Element.ALIGN_RIGHT
        };
        doc.Add(totalParagraph);

        doc.Close();

        return File(ms.ToArray(), "application/pdf", $"Invoice_{lecturer.Name}.pdf");
    }

}
}
