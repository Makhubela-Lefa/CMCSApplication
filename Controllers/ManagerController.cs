using CMCSApplication.Data;
using CMCSApplication.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCSApplication.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ManagerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DASHBOARD
        public IActionResult Index()
        {
            ViewBag.RecentClaims = _context.Claims
                .Where(c => !c.IsDeleted)
                .OrderByDescending(c => c.DateSubmitted)
                .Take(5)
                .ToList();

            ViewBag.PendingCount = _context.Claims
                .Count(c => !c.IsDeleted && c.Status == "Verified by Coordinator");

            ViewBag.RejectedCount = _context.Claims
                .Count(c => !c.IsDeleted &&
                    (c.Status == "Rejected by Coordinator" || c.Status == "Rejected by Manager"));

            ViewBag.ApprovedCount = _context.Claims
                .Count(c => !c.IsDeleted && c.Status == "Approved by Manager");

            return View();
        }

        // CLAIMS WAITING FOR APPROVAL
        public IActionResult Approval()
        {
            var claims = _context.Claims
                .Where(c => !c.IsDeleted
                    && c.CoordinatorStatus == "Approved"
                    && c.ManagerStatus == "Pending Approval")
                .OrderByDescending(c => c.DateSubmitted)
                .ToList();

            return View(claims);
        }

        // REVIEW INDIVIDUAL CLAIM
        public IActionResult Review(int id)
        {
            var claim = _context.Claims
                .FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            return View(claim);
        }

        // APPROVE CLAIM
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
            claim.Status = "Fully Approved";
            claim.ManagerId = User.Identity!.Name!;
            claim.DateApproved = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Claim approved successfully!";
            return RedirectToAction("Approval");
        }

        // REJECT CLAIM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var claim = _context.Claims.FirstOrDefault(c => c.Id == id && !c.IsDeleted);

            if (claim == null)
            {
                TempData["Error"] = "Claim not found.";
                return RedirectToAction("Approval");
            }

            claim.Status = "Rejected by Manager";
            claim.ManagerStatus = "Rejected";
            claim.ManagerId = User.Identity!.Name!;
            claim.DateApproved = DateTime.Now;

            _context.SaveChanges();

            TempData["Info"] = "Claim rejected.";
            return RedirectToAction("Approval");
        }

        // MANAGER REPORTS PAGE
        public IActionResult Reports()
        {
            var claims = _context.Claims
                .Where(c => !c.IsDeleted && c.ManagerStatus == "Approved")
                .ToList();

            ViewBag.TotalClaims = claims.Count;
            ViewBag.TotalPayout = claims.Sum(c => c.TotalAmount);
            ViewBag.TotalHours = claims.Sum(c => c.HoursWorked);

            ViewBag.DepartmentTotals = claims
                .GroupBy(c => c.Department)
                .Select(g => new
                {
                    Department = g.Key,
                    TotalAmount = g.Sum(c => c.TotalAmount),
                    TotalHours = g.Sum(c => c.HoursWorked)
                })
                .ToList();

            return View();
        }

        // PDF DOWNLOAD
        public IActionResult DownloadReportPdf()
        {
            var claims = _context.Claims
                .Where(c => !c.IsDeleted && c.ManagerStatus == "Approved")
                .ToList();

            using var ms = new MemoryStream();
            var document = new iTextSharp.text.Document();
            iTextSharp.text.pdf.PdfWriter.GetInstance(document, ms);

            document.Open();

            var titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 16, iTextSharp.text.Font.BOLD);
            document.Add(new iTextSharp.text.Paragraph("Monthly Claim Report", titleFont));
            document.Add(new iTextSharp.text.Paragraph("\n"));

            document.Add(new iTextSharp.text.Paragraph($"Total Claims: {claims.Count}"));
            document.Add(new iTextSharp.text.Paragraph($"Total Payout: R {claims.Sum(c => c.TotalAmount):N2}"));
            document.Add(new iTextSharp.text.Paragraph($"Total Hours: {claims.Sum(c => c.HoursWorked)} hrs"));
            document.Add(new iTextSharp.text.Paragraph("\n"));

            var table = new iTextSharp.text.pdf.PdfPTable(5) { WidthPercentage = 100 };
            table.AddCell("Lecturer");
            table.AddCell("Department");
            table.AddCell("Month");
            table.AddCell("Hours");
            table.AddCell("Amount");

            foreach (var c in claims)
            {
                table.AddCell(c.LecturerName);
                table.AddCell(c.Department);
                table.AddCell(c.Month);
                table.AddCell(c.HoursWorked.ToString());
                table.AddCell($"R {c.TotalAmount:N2}");
            }

            document.Add(table);
            document.Close();

            return File(ms.ToArray(), "application/pdf", "MonthlyClaimReport.pdf");
        }
    }
}
