using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;
using WebApplication3.Services;

namespace WebApplication3.Controllers
{
    public class CreditsController : Controller
    {
        private readonly BankContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPdfService _pdfService;
        private readonly ICreditScoringService _scoringService;

        public CreditsController(
            BankContext context, 
            UserManager<ApplicationUser> userManager,
            IPdfService pdfService,
            ICreditScoringService scoringService)
        {
            _context = context;
            _userManager = userManager;
            _pdfService = pdfService;
            _scoringService = scoringService;
        }

        // GET: Credits
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var credits = await _context.Credits.ToListAsync();
            return View(credits);
        }

        // GET: Credits/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credit = await _context.Credits
                .FirstOrDefaultAsync(m => m.Id == id);

            if (credit == null)
            {
                return NotFound();
            }

            return View(credit);
        }

        // GET: Credits/Calculator
        [AllowAnonymous]
        public async Task<IActionResult> Calculator()
        {
            ViewBag.Credits = await _context.Credits.ToListAsync();
            return View();
        }

        // GET: Credits/Apply/5
        [Authorize]
        public async Task<IActionResult> Apply(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var credit = await _context.Credits.FindAsync(id);

            if (credit == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            
            // Перевірка чорного списку
            var isBlacklisted = await _scoringService.IsInBlacklist(
                user?.TaxNumber ?? "", 
                user?.Email, 
                user?.PhoneNumber);

            if (isBlacklisted)
            {
                TempData["ErrorMessage"] = "На жаль, ви не можете подати заявку на кредит. Зверніться до менеджера для уточнення деталей.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Credit = credit;

            var application = new CreditApplication
            {
                CreditId = credit.Id,
                Amount = credit.MinAmount,
                TermMonths = credit.MinTermMonths,
                CustomerName = user?.FullName ?? string.Empty,
                Phone = user?.PhoneNumber ?? string.Empty,
                Email = user?.Email ?? string.Empty
            };

            return View(application);
        }

        // POST: Credits/Apply
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(CreditApplication application)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var isBlacklisted = await _scoringService.IsInBlacklist(
                    user?.TaxNumber ?? "", 
                    user?.Email, 
                    user?.PhoneNumber);

                if (isBlacklisted)
                {
                    TempData["ErrorMessage"] = "На жаль, ви не можете подати заявку на кредит.";
                    return RedirectToAction(nameof(Index));
                }
                
                var canApply = await _scoringService.CanApplyForCredit(user!.Id, application.Amount);

                if (!canApply)
                {
                    var creditScore = await _context.CreditScores
                        .FirstOrDefaultAsync(cs => cs.UserId == user.Id);

                    TempData["ErrorMessage"] = $"На жаль, запитувана сума ({application.Amount:N0} грн) перевищує рекомендовану для вас ({creditScore?.RecommendedMaxAmount:N0} грн). " +
                        $"Ваш кредитний рейтинг: {creditScore?.Rating}. Спробуйте зменшити суму або термін кредиту.";
                    
                    var credit = await _context.Credits.FindAsync(application.CreditId);
                    ViewBag.Credit = credit;
                    ViewBag.CreditScore = creditScore;
                    
                    return View(application);
                }

                application.Id = 0;
                application.ApplicationDate = DateTime.Now;
                application.Status = ApplicationStatus.New;
                application.UserId = user?.Id;

                _context.CreditApplications.Add(application);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Success), new { id = application.Id });
            }

            var creditData = await _context.Credits.FindAsync(application.CreditId);
            ViewBag.Credit = creditData;

            return View(application);
        }

        // GET: Credits/CheckEligibility
        [Authorize]
        public async Task<IActionResult> CheckEligibility()
        {
            var user = await _userManager.GetUserAsync(User);
            
            // Розрахунок кредитного скорингу
            var creditScore = await _scoringService.CalculateCreditScore(user!.Id);
            
            // Перевірка чорного списку
            var isBlacklisted = await _scoringService.IsInBlacklist(
                user.TaxNumber ?? "", 
                user.Email, 
                user.PhoneNumber);

            ViewBag.IsBlacklisted = isBlacklisted;

            return View(creditScore);
        }

        // GET: Credits/Success/5
        [Authorize]
        public async Task<IActionResult> Success(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var application = await _context.CreditApplications
                .Include(a => a.Credit)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            return View(application);
        }

        // GET: Credits/MyApplications
        [Authorize]
        public async Task<IActionResult> MyApplications()
        {
            var user = await _userManager.GetUserAsync(User);
            
            var applications = await _context.CreditApplications
                .Include(a => a.Credit)
                .Where(a => a.UserId == user!.Id)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();

            return View(applications);
        }

        // GET: Credits/DownloadApplication/5
        [Authorize]
        public async Task<IActionResult> DownloadApplication(int id)
        {
            var application = await _context.CreditApplications
                .Include(a => a.Credit)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            if (!isAdmin && application.UserId != user?.Id)
            {
                return Forbid();
            }

            var pdfBytes = _pdfService.GenerateCreditApplicationPdf(application);
            return File(pdfBytes, "application/pdf", $"Application_{application.Id}.pdf");
        }

        // GET: Credits/DownloadSchedule/5
        [Authorize]
        public async Task<IActionResult> DownloadSchedule(int id)
        {
            var application = await _context.CreditApplications
                .Include(a => a.Credit)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("Manager");
            
            if (!isAdmin && application.UserId != user?.Id)
            {
                return Forbid();
            }

            var pdfBytes = _pdfService.GeneratePaymentSchedulePdf(application);
            return File(pdfBytes, "application/pdf", $"Schedule_{application.Id}.pdf");
        }
    }
}