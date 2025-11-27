using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public enum BlacklistReason
    {
        [Display(Name = "Несплата боргу")]
        NonPayment,
        
        [Display(Name = "Шахрайство")]
        Fraud,
        
        [Display(Name = "Фальсифікація документів")]
        DocumentFalsification,
        
        [Display(Name = "Порушення договору")]
        ContractViolation,
        
        [Display(Name = "Кредитне шахрайство")]
        CreditFraud,
        
        [Display(Name = "Інше")]
        Other
    }
    
    public class BlacklistEntry
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "ПІБ")]
        public string FullName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "ІПН")]
        [StringLength(10, MinimumLength = 10)]
        public string TaxNumber { get; set; } = string.Empty;
        
        [Display(Name = "Телефон")]
        public string? Phone { get; set; }
        
        [Display(Name = "Email")]
        public string? Email { get; set; }
        
        [Display(Name = "Дата народження")]
        public DateTime? DateOfBirth { get; set; }
        
        [Required]
        [Display(Name = "Причина блокування")]
        public BlacklistReason Reason { get; set; }
        
        [Required]
        [Display(Name = "Детальний опис")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Сума боргу")]
        public decimal? DebtAmount { get; set; }
        
        [Display(Name = "Дата додавання")]
        public DateTime AddedDate { get; set; } = DateTime.Now;
        
        [Display(Name = "Додав")]
        public string AddedBy { get; set; } = string.Empty;
        
        [Display(Name = "Активний")]
        public bool IsActive { get; set; } = true;
        
        [Display(Name = "Дата зняття з блокування")]
        public DateTime? RemovedDate { get; set; }
        
        [Display(Name = "Причина зняття")]
        public string? RemovalReason { get; set; }
        
        [Display(Name = "Примітки")]
        public string? Notes { get; set; }
    }
}