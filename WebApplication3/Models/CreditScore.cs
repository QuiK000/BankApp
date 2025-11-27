using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class CreditScore
    {
        public int Id { get; set; }
        
        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;
        
        [Display(Name = "Загальний бал")]
        [Range(0, 1000)]
        public int TotalScore { get; set; }
        
        [Display(Name = "Бал за доходом")]
        public int IncomeScore { get; set; }
        
        [Display(Name = "Бал за кредитною історією")]
        public int CreditHistoryScore { get; set; }
        
        [Display(Name = "Бал за віком")]
        public int AgeScore { get; set; }
        
        [Display(Name = "Бал за зайнятістю")]
        public int EmploymentScore { get; set; }
        
        [Display(Name = "Бал за існуючими боргами")]
        public int ExistingDebtsScore { get; set; }
        
        [Display(Name = "Рейтинг")]
        public string Rating { get; set; } = string.Empty; // A+, A, B, C, D, E
        
        [Display(Name = "Ймовірність дефолту (%)")]
        public decimal DefaultProbability { get; set; }
        
        [Display(Name = "Рекомендована максимальна сума")]
        public decimal RecommendedMaxAmount { get; set; }
        
        [Display(Name = "Дата розрахунку")]
        public DateTime CalculationDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Примітки")]
        public string? Notes { get; set; }
    }
}