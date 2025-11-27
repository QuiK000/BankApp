using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public enum ApplicationStatus
    {
        [Display(Name = "Нова заявка")]
        New,
        
        [Display(Name = "На розгляді")]
        UnderReview,
        
        [Display(Name = "Потрібні документи")]
        DocumentsRequired,
        
        [Display(Name = "Перевірка документів")]
        DocumentsVerification,
        
        [Display(Name = "Схвалено")]
        Approved,
        
        [Display(Name = "Відхилено")]
        Rejected,
        
        [Display(Name = "Видано")]
        Issued,
        
        [Display(Name = "Скасовано")]
        Cancelled
    }
    
    public class CreditApplication
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Введіть ваше ім'я")]
        [Display(Name = "Ім'я")]
        public string CustomerName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Введіть ваш телефон")]
        [Phone(ErrorMessage = "Невірний формат телефону")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Введіть вашу електронну пошту")]
        [EmailAddress(ErrorMessage = "Невірний формат email")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Сума кредиту")]
        public decimal Amount { get; set; }
        
        [Required]
        [Display(Name = "Термін (місяців)")]
        public int TermMonths { get; set; }
        
        [Display(Name = "Дата подання")]
        public DateTime ApplicationDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Статус")]
        public ApplicationStatus Status { get; set; } = ApplicationStatus.New;
        
        [Display(Name = "Коментар менеджера")]
        public string? ManagerComment { get; set; }
        
        [Display(Name = "Дата зміни статусу")]
        public DateTime? StatusChangeDate { get; set; }
        
        public int CreditId { get; set; }
        public Credit? Credit { get; set; }
        
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        [Display(Name = "Щомісячний платіж")]
        public decimal MonthlyPayment
        {
            get
            {
                if (Credit == null || TermMonths == 0) return 0;
                
                var monthlyRate = Credit.InterestRate / 100 / 12;
                if (monthlyRate == 0) return Amount / TermMonths;
                
                return Amount * monthlyRate * (decimal)Math.Pow((double)(1 + monthlyRate), TermMonths) 
                       / (decimal)(Math.Pow((double)(1 + monthlyRate), TermMonths) - 1);
            }
        }
        
        [Display(Name = "Загальна сума виплат")]
        public decimal TotalAmount => MonthlyPayment * TermMonths;
        
        [Display(Name = "Переплата")]
        public decimal Overpayment => TotalAmount - Amount;
    }
}