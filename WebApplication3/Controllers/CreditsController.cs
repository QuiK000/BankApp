using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;

namespace WebApplication3.Controllers;

public class CreditsController(BankContext context) : Controller
{
    // GET: Credits
    public async Task<IActionResult> Index()
    {
        var credits = await context.Credits.ToListAsync();
        return View(credits);
    }

    // GET: Credits/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var credit = await context.Credits
            .FirstOrDefaultAsync(m => m.Id == id);

        if (credit == null)
        {
            return NotFound();
        }

        return View(credit);
    }

    // GET: Credits/Apply/5
    public async Task<IActionResult> Apply(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var credit = await context.Credits.FindAsync(id);

        if (credit == null)
        {
            return NotFound();
        }

        ViewBag.Credit = credit;

        var application = new CreditApplication
        {
            CreditId = credit.Id,
            Amount = credit.MinAmount,
            TermMonths = credit.MinTermMonths
        };

        return View(application);
    }

    // POST: Credits/Apply
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply(CreditApplication application)
    {
        if (ModelState.IsValid)
        {
            application.ApplicationDate = DateTime.Now;
            application.Status = "Нова заявка";

            context.CreditApplications.Add(application);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Success), new { id = application.Id });
        }

        var credit = await context.Credits.FindAsync(application.CreditId);
        ViewBag.Credit = credit;

        return View(application);
    }

    // GET: Credits/Success/5
    public async Task<IActionResult> Success(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var application = await context.CreditApplications
            .Include(a => a.Credit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (application == null)
        {
            return NotFound();
        }

        return View(application);
    }

    // GET: Credits/MyApplications
    public async Task<IActionResult> MyApplications()
    {
        var applications = await context.CreditApplications
            .Include(a => a.Credit)
            .OrderByDescending(a => a.ApplicationDate)
            .ToListAsync();

        return View(applications);
    }
}