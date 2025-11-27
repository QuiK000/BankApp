using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;
using WebApplication3.ViewModels;

namespace WebApplication3.Services
{
    public interface IReportService
    {
        Task<DashboardViewModel> GetDashboardStatistics();
        Task<IEnumerable<CreditApplication>> GetApplicationsByPeriod(DateTime startDate, DateTime endDate);
        Task<Dictionary<ApplicationStatus, int>> GetApplicationsByStatus();
        Task<Dictionary<string, decimal>> GetApplicationsByCreditType();
    }

    public class ReportService : IReportService
    {
        private readonly BankContext _context;

        public ReportService(BankContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardStatistics()
        {
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var model = new DashboardViewModel
            {
                // Загальна статистика
                TotalApplications = await _context.CreditApplications.CountAsync(),
                TotalClients = await _context.Users.CountAsync(u => u.CreditApplications.Any()),
                TotalAmount = await _context.CreditApplications.SumAsync(a => a.Amount),
                
                // Статистика за місяць
                MonthApplications = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfMonth),
                MonthAmount = await _context.CreditApplications
                    .Where(a => a.ApplicationDate >= startOfMonth)
                    .SumAsync(a => a.Amount),
                
                // Статистика за рік
                YearApplications = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfYear),
                YearAmount = await _context.CreditApplications
                    .Where(a => a.ApplicationDate >= startOfYear)
                    .SumAsync(a => a.Amount),
                
                // За статусами
                NewApplications = await _context.CreditApplications
                    .CountAsync(a => a.Status == ApplicationStatus.New),
                UnderReviewApplications = await _context.CreditApplications
                    .CountAsync(a => a.Status == ApplicationStatus.UnderReview),
                ApprovedApplications = await _context.CreditApplications
                    .CountAsync(a => a.Status == ApplicationStatus.Approved),
                RejectedApplications = await _context.CreditApplications
                    .CountAsync(a => a.Status == ApplicationStatus.Rejected),
                
                // Популярні кредити
                PopularCredits = await _context.CreditApplications
                    .Include(a => a.Credit)
                    .GroupBy(a => a.Credit!.Name)
                    .Select(g => new CreditPopularity
                    {
                        CreditName = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(a => a.Amount)
                    })
                    .OrderByDescending(c => c.Count)
                    .Take(5)
                    .ToListAsync(),
                
                // Останні заявки
                RecentApplications = await _context.CreditApplications
                    .Include(a => a.Credit)
                    .OrderByDescending(a => a.ApplicationDate)
                    .Take(10)
                    .ToListAsync()
            };

            // Середня сума заявки
            if (model.TotalApplications > 0)
            {
                model.AverageAmount = model.TotalAmount / model.TotalApplications;
            }

            return model;
        }

        public async Task<IEnumerable<CreditApplication>> GetApplicationsByPeriod(DateTime startDate, DateTime endDate)
        {
            return await _context.CreditApplications
                .Include(a => a.Credit)
                .Include(a => a.User)
                .Where(a => a.ApplicationDate >= startDate && a.ApplicationDate <= endDate)
                .OrderByDescending(a => a.ApplicationDate)
                .ToListAsync();
        }

        public async Task<Dictionary<ApplicationStatus, int>> GetApplicationsByStatus()
        {
            var statuses = Enum.GetValues<ApplicationStatus>();
            var result = new Dictionary<ApplicationStatus, int>();

            foreach (var status in statuses)
            {
                var count = await _context.CreditApplications.CountAsync(a => a.Status == status);
                result[status] = count;
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetApplicationsByCreditType()
        {
            return await _context.CreditApplications
                .Include(a => a.Credit)
                .GroupBy(a => a.Credit!.Name)
                .Select(g => new { CreditName = g.Key, TotalAmount = g.Sum(a => a.Amount) })
                .ToDictionaryAsync(x => x.CreditName, x => x.TotalAmount);
        }
    }
}