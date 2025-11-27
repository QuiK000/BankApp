using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;
using WebApplication3.Services;
using WebApplication3.ViewModels;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AdminController : Controller
    {
        private readonly BankContext _context;
        private readonly IReportService _reportService;
        private readonly IPdfService _pdfService;

        public AdminController(
            BankContext context,
            IReportService reportService,
            IPdfService pdfService)
        {
            _context = context;
            _reportService = reportService;
            _pdfService = pdfService;
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var statistics = await _reportService.GetDashboardStatistics();
            return View(statistics);
        }

        // GET: Admin/Applications
        public async Task<IActionResult> Applications(ApplicationFilterViewModel filter)
        {
            var query = _context.CreditApplications
                .Include(a => a.Credit)
                .Include(a => a.User)
                .AsQueryable();

            // Фільтрація
            if (filter.StartDate.HasValue)
            {
                query = query.Where(a => a.ApplicationDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(a => a.ApplicationDate <= filter.EndDate.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(a => a.Status == filter.Status.Value);
            }

            if (filter.CreditId.HasValue)
            {
                query = query.Where(a => a.CreditId == filter.CreditId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                query = query.Where(a => 
                    a.CustomerName.Contains(filter.SearchTerm) ||
                    a.Phone.Contains(filter.SearchTerm) ||
                    a.Email.Contains(filter.SearchTerm));
            }

            var applications = await query
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            ViewBag.Filter = filter;
            ViewBag.Credits = await _context.Credits.ToListAsync();
            ViewBag.Statuses = Enum.GetValues<ApplicationStatus>();

            return View(applications);
        }

        // GET: Admin/ApplicationDetails/5
        public async Task<IActionResult> ApplicationDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.CreditApplications
                .Include(a => a.Credit)
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // POST: Admin/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus status, string? comment)
        {
            var application = await _context.CreditApplications.FindAsync(id);
            
            if (application == null)
            {
                return NotFound();
            }

            application.Status = status;
            application.StatusChangeDate = DateTime.Now;
            application.ManagerComment = comment;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Статус заявки успішно оновлено";
            return RedirectToAction(nameof(ApplicationDetails), new { id });
        }

        // GET: Admin/Reports
        public IActionResult Reports()
        {
            return View();
        }

        // POST: Admin/GenerateReport
        [HttpPost]
        public async Task<IActionResult> GenerateReport(DateTime startDate, DateTime endDate)
        {
            var applications = await _reportService.GetApplicationsByPeriod(startDate, endDate);
            var pdfBytes = _pdfService.GenerateApplicationsReportPdf(applications);
            
            return File(pdfBytes, "application/pdf", $"Report_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.pdf");
        }

        // GET: Admin/Statistics
        public async Task<IActionResult> Statistics()
        {
            var statusStats = await _reportService.GetApplicationsByStatus();
            var creditStats = await _reportService.GetApplicationsByCreditType();

            ViewBag.StatusStats = statusStats;
            ViewBag.CreditStats = creditStats;

            return View();
        }

        // API для графіків
        [HttpGet]
        public async Task<JsonResult> GetApplicationsChart()
        {
            var last6Months = Enumerable.Range(0, 6)
                .Select(i => DateTime.Now.AddMonths(-i))
                .Reverse()
                .ToList();

            var data = new List<object>();

            foreach (var month in last6Months)
            {
                var startOfMonth = new DateTime(month.Year, month.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

                var count = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfMonth && a.ApplicationDate <= endOfMonth);

                var amount = await _context.CreditApplications
                    .Where(a => a.ApplicationDate >= startOfMonth && a.ApplicationDate <= endOfMonth)
                    .SumAsync(a => a.Amount);

                data.Add(new
                {
                    month = month.ToString("MMM yyyy"),
                    count = count,
                    amount = amount
                });
            }

            return Json(data);
        }

        [HttpGet]
        public async Task<JsonResult> GetStatusChart()
        {
            var statuses = Enum.GetValues<ApplicationStatus>();
            var data = new List<object>();

            foreach (var status in statuses)
            {
                var count = await _context.CreditApplications.CountAsync(a => a.Status == status);
                if (count > 0)
                {
                    data.Add(new
                    {
                        status = status.ToString(),
                        count = count
                    });
                }
            }

            return Json(data);
        }

        [HttpGet]
        public async Task<JsonResult> GetCreditTypesChart()
        {
            var data = await _context.CreditApplications
                .Include(a => a.Credit)
                .GroupBy(a => a.Credit!.Name)
                .Select(g => new
                {
                    creditType = g.Key,
                    count = g.Count(),
                    amount = g.Sum(a => a.Amount)
                })
                .OrderByDescending(x => x.count)
                .ToListAsync();

            return Json(data);
        }
    }
}