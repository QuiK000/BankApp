using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication3.db;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class AnalyticsController : Controller
    {
        private readonly BankContext _context;

        public AnalyticsController(BankContext context)
        {
            _context = context;
        }

        // GET: Analytics/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Now.Date;
            var startOfWeek = today.AddDays(-(int)today.DayOfWeek + 1);
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            var startOfYear = new DateTime(today.Year, 1, 1);

            var model = new AnalyticsDashboardViewModel
            {
                // Сьогодні
                TodayApplications = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate.Date == today),
                TodayAmount = await _context.CreditApplications
                    .Where(a => a.ApplicationDate.Date == today)
                    .SumAsync(a => (decimal?)a.Amount) ?? 0,
                TodayApproved = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate.Date == today && a.Status == ApplicationStatus.Approved),

                // За тиждень
                WeekApplications = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfWeek),
                WeekAmount = await _context.CreditApplications
                    .Where(a => a.ApplicationDate >= startOfWeek)
                    .SumAsync(a => (decimal?)a.Amount) ?? 0,
                WeekApproved = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfWeek && a.Status == ApplicationStatus.Approved),

                // За місяць
                MonthApplications = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfMonth),
                MonthAmount = await _context.CreditApplications
                    .Where(a => a.ApplicationDate >= startOfMonth)
                    .SumAsync(a => (decimal?)a.Amount) ?? 0,
                MonthApproved = await _context.CreditApplications
                    .CountAsync(a => a.ApplicationDate >= startOfMonth && a.Status == ApplicationStatus.Approved),

                // Конверсія
                ConversionRate = await CalculateConversionRate(),
                AverageProcessingTime = await CalculateAverageProcessingTime(),
                
                // Топ менеджери
                TopManagers = await GetTopManagers(),
                
                // Години пік
                PeakHours = await GetPeakHours()
            };

            return View(model);
        }

        // GET: Analytics/CreditPerformance
        public async Task<IActionResult> CreditPerformance()
        {
            var credits = await _context.Credits.ToListAsync();
            var performance = new List<CreditPerformanceModel>();

            foreach (var credit in credits)
            {
                var apps = await _context.CreditApplications
                    .Where(a => a.CreditId == credit.Id)
                    .ToListAsync();

                var approved = apps.Count(a => a.Status == ApplicationStatus.Approved);
                var total = apps.Count;

                performance.Add(new CreditPerformanceModel
                {
                    CreditName = credit.Name,
                    TotalApplications = total,
                    ApprovedApplications = approved,
                    RejectedApplications = apps.Count(a => a.Status == ApplicationStatus.Rejected),
                    TotalAmount = apps.Sum(a => a.Amount),
                    ApprovalRate = total > 0 ? (approved * 100.0 / total) : 0,
                    AverageAmount = total > 0 ? apps.Average(a => a.Amount) : 0,
                    AverageTerm = total > 0 ? apps.Average(a => a.TermMonths) : 0
                });
            }

            return View(performance);
        }

        // GET: Analytics/ClientAnalysis
        public async Task<IActionResult> ClientAnalysis()
        {
            var users = await _context.Users
                .Include(u => u.CreditApplications)
                .ToListAsync();

            var model = new ClientAnalysisViewModel
            {
                TotalClients = users.Count,
                ActiveClients = users.Count(u => u.CreditApplications.Any(a => 
                    a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Issued)),
                NewClientsThisMonth = users.Count(u => u.RegistrationDate >= DateTime.Now.AddMonths(-1)),
                RepeatClients = users.Count(u => u.CreditApplications.Count > 1),
                
                ClientsByAge = await GetClientsByAge(),
                ClientsByRegion = await GetClientsByRegion(),
                ClientsByCreditCount = await GetClientsByCreditCount()
            };

            return View(model);
        }

        // GET: Analytics/TimeAnalysis
        public async Task<IActionResult> TimeAnalysis(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-3);
            var end = endDate ?? DateTime.Now;

            var applications = await _context.CreditApplications
                .Where(a => a.ApplicationDate >= start && a.ApplicationDate <= end)
                .ToListAsync();

            var model = new TimeAnalysisViewModel
            {
                StartDate = start,
                EndDate = end,
                DailyStats = GetDailyStats(applications),
                HourlyStats = GetHourlyStats(applications),
                WeekdayStats = GetWeekdayStats(applications),
                MonthlyTrends = GetMonthlyTrends(applications)
            };

            return View(model);
        }

        // API: Експорт даних
        [HttpPost]
        public async Task<IActionResult> ExportData(string reportType, DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Now.AddMonths(-1);
            var end = endDate ?? DateTime.Now;

            var data = await _context.CreditApplications
                .Include(a => a.Credit)
                .Include(a => a.User)
                .Where(a => a.ApplicationDate >= start && a.ApplicationDate <= end)
                .ToListAsync();

            // Генерація CSV
            var csv = GenerateCSV(data, reportType);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            
            return File(bytes, "text/csv", $"Export_{reportType}_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }

        // Helper Methods
        private async Task<double> CalculateConversionRate()
        {
            var total = await _context.CreditApplications.CountAsync();
            if (total == 0) return 0;

            var approved = await _context.CreditApplications
                .CountAsync(a => a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Issued);

            return Math.Round((approved * 100.0 / total), 2);
        }

        private async Task<double> CalculateAverageProcessingTime()
        {
            var processed = await _context.CreditApplications
                .Where(a => a.StatusChangeDate.HasValue && 
                           (a.Status == ApplicationStatus.Approved || a.Status == ApplicationStatus.Rejected))
                .ToListAsync();

            if (!processed.Any()) return 0;

            var averageHours = processed
                .Average(a => (a.StatusChangeDate!.Value - a.ApplicationDate).TotalHours);

            return Math.Round(averageHours, 1);
        }

        private async Task<List<ManagerPerformance>> GetTopManagers()
        {
            var managers = await _context.CreditApplications
                .Where(a => a.StatusChangeDate.HasValue && !string.IsNullOrEmpty(a.ManagerComment))
                .GroupBy(a => a.ManagerComment)
                .Select(g => new ManagerPerformance
                {
                    ManagerName = "Менеджер",
                    ProcessedApplications = g.Count(),
                    ApprovedApplications = g.Count(a => a.Status == ApplicationStatus.Approved)
                })
                .OrderByDescending(m => m.ProcessedApplications)
                .Take(5)
                .ToListAsync();

            return managers;
        }

        private async Task<Dictionary<int, int>> GetPeakHours()
        {
            var apps = await _context.CreditApplications.ToListAsync();
            return apps
                .GroupBy(a => a.ApplicationDate.Hour)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<Dictionary<string, int>> GetClientsByAge()
        {
            var users = await _context.Users.Where(u => u.DateOfBirth.HasValue).ToListAsync();
            var ageGroups = new Dictionary<string, int>
            {
                ["18-25"] = 0,
                ["26-35"] = 0,
                ["36-45"] = 0,
                ["46-55"] = 0,
                ["56+"] = 0
            };

            foreach (var user in users)
            {
                var age = DateTime.Now.Year - user.DateOfBirth!.Value.Year;
                if (age < 26) ageGroups["18-25"]++;
                else if (age < 36) ageGroups["26-35"]++;
                else if (age < 46) ageGroups["36-45"]++;
                else if (age < 56) ageGroups["46-55"]++;
                else ageGroups["56+"]++;
            }

            return ageGroups;
        }

        private async Task<Dictionary<string, int>> GetClientsByRegion()
        {
            var users = await _context.Users.Where(u => !string.IsNullOrEmpty(u.Address)).ToListAsync();
            return users
                .GroupBy(u => u.Address?.Split(',').FirstOrDefault()?.Trim() ?? "Не вказано")
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private async Task<Dictionary<int, int>> GetClientsByCreditCount()
        {
            var users = await _context.Users.Include(u => u.CreditApplications).ToListAsync();
            return users
                .GroupBy(u => u.CreditApplications.Count)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private List<DailyStat> GetDailyStats(List<CreditApplication> applications)
        {
            return applications
                .GroupBy(a => a.ApplicationDate.Date)
                .Select(g => new DailyStat
                {
                    Date = g.Key,
                    Count = g.Count(),
                    Amount = g.Sum(a => a.Amount),
                    Approved = g.Count(a => a.Status == ApplicationStatus.Approved)
                })
                .OrderBy(s => s.Date)
                .ToList();
        }

        private Dictionary<int, int> GetHourlyStats(List<CreditApplication> applications)
        {
            return applications
                .GroupBy(a => a.ApplicationDate.Hour)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private Dictionary<string, int> GetWeekdayStats(List<CreditApplication> applications)
        {
            return applications
                .GroupBy(a => a.ApplicationDate.DayOfWeek.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        private List<MonthlyTrend> GetMonthlyTrends(List<CreditApplication> applications)
        {
            return applications
                .GroupBy(a => new { a.ApplicationDate.Year, a.ApplicationDate.Month })
                .Select(g => new MonthlyTrend
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMMM yyyy"),
                    Applications = g.Count(),
                    Amount = g.Sum(a => a.Amount),
                    ApprovalRate = g.Count() > 0 ? (g.Count(a => a.Status == ApplicationStatus.Approved) * 100.0 / g.Count()) : 0
                })
                .ToList();
        }

        private string GenerateCSV(List<CreditApplication> data, string reportType)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("ID,Дата,Клієнт,Кредит,Сума,Термін,Статус,Email,Телефон");

            foreach (var app in data)
            {
                csv.AppendLine($"{app.Id},{app.ApplicationDate:yyyy-MM-dd},{app.CustomerName}," +
                              $"{app.Credit?.Name},{app.Amount},{app.TermMonths},{app.Status}," +
                              $"{app.Email},{app.Phone}");
            }

            return csv.ToString();
        }
    }

    // ViewModels
    public class AnalyticsDashboardViewModel
    {
        public int TodayApplications { get; set; }
        public decimal TodayAmount { get; set; }
        public int TodayApproved { get; set; }
        public int WeekApplications { get; set; }
        public decimal WeekAmount { get; set; }
        public int WeekApproved { get; set; }
        public int MonthApplications { get; set; }
        public decimal MonthAmount { get; set; }
        public int MonthApproved { get; set; }
        public double ConversionRate { get; set; }
        public double AverageProcessingTime { get; set; }
        public List<ManagerPerformance> TopManagers { get; set; } = new();
        public Dictionary<int, int> PeakHours { get; set; } = new();
    }

    public class ManagerPerformance
    {
        public string ManagerName { get; set; } = string.Empty;
        public int ProcessedApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public double ApprovalRate => ProcessedApplications > 0 
            ? Math.Round((ApprovedApplications * 100.0 / ProcessedApplications), 1) : 0;
    }

    public class CreditPerformanceModel
    {
        public string CreditName { get; set; } = string.Empty;
        public int TotalApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        public decimal TotalAmount { get; set; }
        public double ApprovalRate { get; set; }
        public decimal AverageAmount { get; set; }
        public double AverageTerm { get; set; }
    }

    public class ClientAnalysisViewModel
    {
        public int TotalClients { get; set; }
        public int ActiveClients { get; set; }
        public int NewClientsThisMonth { get; set; }
        public int RepeatClients { get; set; }
        public Dictionary<string, int> ClientsByAge { get; set; } = new();
        public Dictionary<string, int> ClientsByRegion { get; set; } = new();
        public Dictionary<int, int> ClientsByCreditCount { get; set; } = new();
    }

    public class TimeAnalysisViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<DailyStat> DailyStats { get; set; } = new();
        public Dictionary<int, int> HourlyStats { get; set; } = new();
        public Dictionary<string, int> WeekdayStats { get; set; } = new();
        public List<MonthlyTrend> MonthlyTrends { get; set; } = new();
    }

    public class DailyStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
        public int Approved { get; set; }
    }

    public class MonthlyTrend
    {
        public string Month { get; set; } = string.Empty;
        public int Applications { get; set; }
        public decimal Amount { get; set; }
        public double ApprovalRate { get; set; }
    }
}