using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;

namespace WebApplication3.Controllers;

public class CustomersController(BankContext context) : Controller
{
    // GET: Customers
    public async Task<IActionResult> Index()
    {
        var customers = await context.Customers
            .Include(c => c.CustomerCredits)
            .ThenInclude(cc => cc.Credit)
            .ToListAsync();

        return View(customers);
    }

    // GET: Customers/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var customer = await context.Customers
            .Include(c => c.CustomerCredits)
            .ThenInclude(cc => cc.Credit)
            .Include(c => c.CustomerServices)
            .ThenInclude(cs => cs.Service)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (customer == null) return NotFound();

        return View(customer);
    }

    // GET: Customers/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Customers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
        if (ModelState.IsValid)
        {
            customer.RegistrationDate = DateTime.Now;
            customer.Status = "Активний";

            context.Add(customer);
            await context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(customer);
    }

    // GET: Customers/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var customer = await context.Customers
            .Include(c => c.CustomerCredits)
            .Include(c => c.CustomerServices)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null) return NotFound();
        
        ViewBag.Credits = await context.Credits.ToListAsync();
        ViewBag.Services = await context.Services.Where(s => s.IsActive).ToListAsync();
        ViewBag.SelectedCreditIds = customer.CustomerCredits.Select(cc => cc.CreditId).ToList();
        ViewBag.SelectedServiceIds = customer.CustomerServices.Select(cs => cs.ServiceId).ToList();

        return View(customer);
    }

    // POST: Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Customer customer, int[] selectedCredits, int[] selectedServices)
    {
        if (id != customer.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                context.Update(customer);
                
                var existingCredits = await context.CustomerCredits
                    .Where(cc => cc.CustomerId == id)
                    .ToListAsync();

                var existingServices = await context.CustomerServices
                    .Where(cs => cs.CustomerId == id)
                    .ToListAsync();
                
                if (selectedCredits != null)
                {
                    var creditsToRemove = existingCredits
                        .Where(ec => !selectedCredits.Contains(ec.CreditId))
                        .ToList();
                    context.CustomerCredits.RemoveRange(creditsToRemove);
                    
                    foreach (var creditId in selectedCredits)
                    {
                        if (!existingCredits.Any(ec => ec.CreditId == creditId))
                        {
                            var credit = await context.Credits.FindAsync(creditId);
                            if (credit != null)
                            {
                                context.CustomerCredits.Add(new CustomerCredit
                                {
                                    CustomerId = id,
                                    CreditId = creditId,
                                    ApprovedAmount = credit.MinAmount,
                                    TermMonths = credit.MinTermMonths,
                                    InterestRate = credit.InterestRate,
                                    IssueDate = DateTime.Now,
                                    Status = "Активний",
                                    RemainingDebt = credit.MinAmount
                                });
                            }
                        }
                    }
                }
                else
                {
                    context.CustomerCredits.RemoveRange(existingCredits);
                }
                
                if (selectedServices != null)
                {
                    var servicesToRemove = existingServices
                        .Where(es => !selectedServices.Contains(es.ServiceId))
                        .ToList();
                    context.CustomerServices.RemoveRange(servicesToRemove);

                    foreach (var serviceId in selectedServices)
                    {
                        if (!existingServices.Any(es => es.ServiceId == serviceId))
                        {
                            var service = await context.Services.FindAsync(serviceId);
                            if (service != null)
                            {
                                context.CustomerServices.Add(new CustomerService
                                {
                                    CustomerId = id,
                                    ServiceId = serviceId,
                                    ActivationDate = DateTime.Now,
                                    Status = "Активна",
                                    TotalCost = service.Price
                                });
                            }
                        }
                    }
                }
                else
                {
                    context.CustomerServices.RemoveRange(existingServices);
                }

                await context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = customer.Id });
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(customer.Id))
                    return NotFound();
                throw;
            }
        }

        ViewBag.Credits = await context.Credits.ToListAsync();
        ViewBag.Services = await context.Services.Where(s => s.IsActive).ToListAsync();
        return View(customer);
    }

    // GET: Customers/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var customer = await context.Customers
            .Include(c => c.CustomerCredits)
            .ThenInclude(cc => cc.Credit)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (customer == null) return NotFound();

        return View(customer);
    }

    // POST: Customers/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var customer = await context.Customers.FindAsync(id);
        if (customer != null)
        {
            context.Customers.Remove(customer);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private bool CustomerExists(int id)
    {
        return context.Customers.Any(e => e.Id == id);
    }
}