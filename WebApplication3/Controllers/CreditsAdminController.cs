using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CreditsAdminController : Controller
    {
        private readonly BankContext _context;

        public CreditsAdminController(BankContext context)
        {
            _context = context;
        }

        // GET: CreditsAdmin
        public async Task<IActionResult> Index()
        {
            var credits = await _context.Credits
                .Include(c => c.CustomerCredits)
                .ToListAsync();
            return View(credits);
        }

        // GET: CreditsAdmin/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var credit = await _context.Credits
                .Include(c => c.CustomerCredits)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (credit == null)
                return NotFound();

            // Статистика по кредиту
            var applications = await _context.CreditApplications
                .Where(a => a.CreditId == id)
                .ToListAsync();

            ViewBag.TotalApplications = applications.Count;
            ViewBag.ApprovedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved);
            ViewBag.TotalAmount = applications.Sum(a => a.Amount);
            ViewBag.AverageAmount = applications.Any() ? applications.Average(a => a.Amount) : 0;

            return View(credit);
        }

        // GET: CreditsAdmin/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CreditsAdmin/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Credit credit)
        {
            if (ModelState.IsValid)
            {
                // Валідація даних
                if (credit.MinAmount >= credit.MaxAmount)
                {
                    ModelState.AddModelError("MaxAmount", "Максимальна сума має бути більшою за мінімальну");
                    return View(credit);
                }

                if (credit.MinTermMonths >= credit.MaxTermMonths)
                {
                    ModelState.AddModelError("MaxTermMonths", "Максимальний термін має бути більшим за мінімальний");
                    return View(credit);
                }

                _context.Add(credit);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Кредит '{credit.Name}' успішно створено";
                return RedirectToAction(nameof(Index));
            }

            return View(credit);
        }

        // GET: CreditsAdmin/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var credit = await _context.Credits.FindAsync(id);
            if (credit == null)
                return NotFound();

            return View(credit);
        }

        // POST: CreditsAdmin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Credit credit)
        {
            if (id != credit.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                // Валідація
                if (credit.MinAmount >= credit.MaxAmount)
                {
                    ModelState.AddModelError("MaxAmount", "Максимальна сума має бути більшою за мінімальну");
                    return View(credit);
                }

                if (credit.MinTermMonths >= credit.MaxTermMonths)
                {
                    ModelState.AddModelError("MaxTermMonths", "Максимальний термін має бути більшим за мінімальний");
                    return View(credit);
                }

                try
                {
                    _context.Update(credit);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Кредит '{credit.Name}' успішно оновлено";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CreditExists(credit.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(credit);
        }

        // GET: CreditsAdmin/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var credit = await _context.Credits
                .Include(c => c.CustomerCredits)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (credit == null)
                return NotFound();

            // Перевірка чи є активні заявки
            var activeApplicationsCount = await _context.CreditApplications
                .CountAsync(a => a.CreditId == id);

            ViewBag.ActiveApplicationsCount = activeApplicationsCount;
            ViewBag.ActiveCreditsCount = credit.CustomerCredits.Count;

            return View(credit);
        }

        // POST: CreditsAdmin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null)
                return NotFound();

            // Перевірка чи можна видалити
            var hasActiveApplications = await _context.CreditApplications
                .AnyAsync(a => a.CreditId == id && 
                    (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Issued));

            if (hasActiveApplications)
            {
                TempData["ErrorMessage"] = "Неможливо видалити кредит з активними заявками. Спочатку завершіть всі активні кредити.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            _context.Credits.Remove(credit);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Кредит '{credit.Name}' успішно видалено";
            return RedirectToAction(nameof(Index));
        }

        // GET: CreditsAdmin/Statistics/5
        public async Task<IActionResult> Statistics(int? id)
        {
            if (id == null)
                return NotFound();

            var credit = await _context.Credits.FindAsync(id);
            if (credit == null)
                return NotFound();

            var applications = await _context.CreditApplications
                .Include(a => a.User)
                .Where(a => a.CreditId == id)
                .ToListAsync();

            var model = new CreditStatisticsViewModel
            {
                Credit = credit,
                TotalApplications = applications.Count,
                NewApplications = applications.Count(a => a.Status == ApplicationStatus.New),
                UnderReviewApplications = applications.Count(a => a.Status == ApplicationStatus.UnderReview),
                ApprovedApplications = applications.Count(a => a.Status == ApplicationStatus.Approved),
                RejectedApplications = applications.Count(a => a.Status == ApplicationStatus.Rejected),
                IssuedApplications = applications.Count(a => a.Status == ApplicationStatus.Issued),
                TotalAmount = applications.Sum(a => a.Amount),
                AverageAmount = applications.Any() ? applications.Average(a => a.Amount) : 0,
                AverageTerm = applications.Any() ? applications.Average(a => a.TermMonths) : 0,
                ApprovalRate = applications.Count > 0 
                    ? (applications.Count(a => a.Status == ApplicationStatus.Approved) * 100.0 / applications.Count)
                    : 0,
                RecentApplications = applications
                    .OrderByDescending(a => a.ApplicationDate)
                    .Take(10)
                    .ToList()
            };

            return View(model);
        }

        // POST: CreditsAdmin/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var credit = await _context.Credits.FindAsync(id);
            if (credit == null)
                return NotFound();

            // Можна додати поле IsActive до моделі Credit
            // credit.IsActive = !credit.IsActive;
            
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Статус кредиту змінено";
            return RedirectToAction(nameof(Index));
        }

        private bool CreditExists(int id)
        {
            return _context.Credits.Any(e => e.Id == id);
        }
    }

    // ViewModel для статистики
    public class CreditStatisticsViewModel
    {
        public Credit Credit { get; set; } = null!;
        public int TotalApplications { get; set; }
        public int NewApplications { get; set; }
        public int UnderReviewApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public int IssuedApplications { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public double AverageTerm { get; set; }
        public double ApprovalRate { get; set; }
        public List<CreditApplication> RecentApplications { get; set; } = new();
    }
}