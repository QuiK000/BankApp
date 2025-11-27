using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class BlacklistController : Controller
    {
        private readonly BankContext _context;

        public BlacklistController(BankContext context)
        {
            _context = context;
        }

        // GET: Blacklist
        public async Task<IActionResult> Index(string? searchTerm, bool showInactive = false)
        {
            var query = _context.BlacklistEntries.AsQueryable();

            if (!showInactive)
            {
                query = query.Where(b => b.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(b => 
                    b.FullName.Contains(searchTerm) ||
                    b.TaxNumber.Contains(searchTerm) ||
                    (b.Phone != null && b.Phone.Contains(searchTerm)) ||
                    (b.Email != null && b.Email.Contains(searchTerm)));
            }

            var entries = await query
                .OrderByDescending(b => b.AddedDate)
                .ToListAsync();

            ViewBag.SearchTerm = searchTerm;
            ViewBag.ShowInactive = showInactive;
            ViewBag.ActiveCount = await _context.BlacklistEntries.CountAsync(b => b.IsActive);
            ViewBag.InactiveCount = await _context.BlacklistEntries.CountAsync(b => !b.IsActive);

            return View(entries);
        }

        // GET: Blacklist/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var entry = await _context.BlacklistEntries
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entry == null)
                return NotFound();

            return View(entry);
        }

        // GET: Blacklist/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Blacklist/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlacklistEntry entry)
        {
            if (ModelState.IsValid)
            {
                // Перевірка чи не існує вже запис з таким ІПН
                var existing = await _context.BlacklistEntries
                    .FirstOrDefaultAsync(b => b.TaxNumber == entry.TaxNumber && b.IsActive);

                if (existing != null)
                {
                    ModelState.AddModelError("TaxNumber", 
                        "Особа з таким ІПН вже знаходиться в чорному списку");
                    return View(entry);
                }

                entry.AddedDate = DateTime.Now;
                entry.AddedBy = User.Identity?.Name ?? "System";
                entry.IsActive = true;

                _context.Add(entry);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Запис успішно додано до чорного списку";
                return RedirectToAction(nameof(Index));
            }

            return View(entry);
        }

        // GET: Blacklist/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var entry = await _context.BlacklistEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            return View(entry);
        }

        // POST: Blacklist/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlacklistEntry entry)
        {
            if (id != entry.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(entry);
                    await _context.SaveChangesAsync();
                    
                    TempData["SuccessMessage"] = "Запис успішно оновлено";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntryExists(entry.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(entry);
        }

        // POST: Blacklist/Remove/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id, string removalReason)
        {
            var entry = await _context.BlacklistEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            entry.IsActive = false;
            entry.RemovedDate = DateTime.Now;
            entry.RemovalReason = removalReason;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Особу знято з чорного списку";
            return RedirectToAction(nameof(Index));
        }

        // POST: Blacklist/Restore/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var entry = await _context.BlacklistEntries.FindAsync(id);
            if (entry == null)
                return NotFound();

            entry.IsActive = true;
            entry.RemovedDate = null;
            entry.RemovalReason = null;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Запис відновлено в чорному списку";
            return RedirectToAction(nameof(Index));
        }

        // GET: Blacklist/Check
        public IActionResult Check()
        {
            return View();
        }

        // POST: Blacklist/CheckPerson
        [HttpPost]
        public async Task<IActionResult> CheckPerson(string taxNumber, string? email, string? phone)
        {
            var entries = await _context.BlacklistEntries
                .Where(b => b.IsActive &&
                    (b.TaxNumber == taxNumber ||
                     (email != null && b.Email == email) ||
                     (phone != null && b.Phone == phone)))
                .ToListAsync();

            if (!entries.Any())
            {
                return Json(new { isBlacklisted = false, message = "Особу не знайдено в чорному списку" });
            }

            return Json(new 
            { 
                isBlacklisted = true, 
                message = "УВАГА! Особа знаходиться в чорному списку!",
                entries = entries.Select(e => new {
                    e.FullName,
                    e.Reason,
                    e.Description,
                    e.AddedDate
                })
            });
        }

        // GET: Blacklist/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var entry = await _context.BlacklistEntries
                .FirstOrDefaultAsync(m => m.Id == id);

            if (entry == null)
                return NotFound();

            return View(entry);
        }

        // POST: Blacklist/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var entry = await _context.BlacklistEntries.FindAsync(id);
            if (entry != null)
            {
                _context.BlacklistEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Запис видалено з бази даних";
            return RedirectToAction(nameof(Index));
        }

        private bool EntryExists(int id)
        {
            return _context.BlacklistEntries.Any(e => e.Id == id);
        }
    }
}