using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class DashboardViewModel
    {
        // Загальна статистика
        public int TotalApplications { get; set; }
        public int TotalClients { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        
        // Статистика за місяць
        public int MonthApplications { get; set; }
        public decimal MonthAmount { get; set; }
        
        // Статистика за рік
        public int YearApplications { get; set; }
        public decimal YearAmount { get; set; }
        
        // За статусами
        public int NewApplications { get; set; }
        public int UnderReviewApplications { get; set; }
        public int ApprovedApplications { get; set; }
        public int RejectedApplications { get; set; }
        
        // Популярні кредити
        public List<CreditPopularity> PopularCredits { get; set; } = new();
        
        // Останні заявки
        public List<CreditApplication> RecentApplications { get; set; } = new();
    }

    public class CreditPopularity
    {
        public string CreditName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ApplicationFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ApplicationStatus? Status { get; set; }
        public int? CreditId { get; set; }
        public string? SearchTerm { get; set; }
    }
}