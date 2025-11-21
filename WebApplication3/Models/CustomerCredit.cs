using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class CustomerCredit
    {
        public int Id { get; set; }
        
        public int CustomerId { get; set; }
        public int CreditId { get; set; }
        
        public virtual Customer Customer { get; set; } = null!;
        public virtual Credit Credit { get; set; } = null!;
        
        [Display(Name = "Сума кредиту")]
        public decimal ApprovedAmount { get; set; }
        
        [Display(Name = "Термін (місяців)")]
        public int TermMonths { get; set; }
        
        [Display(Name = "Процентна ставка")]
        public decimal InterestRate { get; set; }
        
        [Display(Name = "Дата видачі")]
        public DateTime IssueDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Дата погашення")]
        public DateTime? RepaymentDate { get; set; }
        
        [Display(Name = "Статус кредиту")]
        public string Status { get; set; } = "Активний";
        
        [Display(Name = "Залишок боргу")]
        public decimal RemainingDebt { get; set; }
        
        [Display(Name = "Щомісячний платіж")]
        public decimal MonthlyPayment
        {
            get
            {
                if (TermMonths == 0) return 0;
                
                var monthlyRate = InterestRate / 100 / 12;
                if (monthlyRate == 0) return ApprovedAmount / TermMonths;
                
                return ApprovedAmount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), TermMonths) / (decimal)(Math.Pow((double)(1 + monthlyRate), TermMonths) - 1);
            }
        }
    }
}