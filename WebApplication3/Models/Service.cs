using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models;

public class Service
{
    public int Id { get; set; }
        
    [Required]
    [Display(Name = "Назва послуги")]
    public string Name { get; set; } = string.Empty;
        
    [Display(Name = "Опис")]
    public string Description { get; set; } = string.Empty;
        
    [Display(Name = "Вартість (грн)")]
    public decimal Price { get; set; }
        
    [Display(Name = "Відсоток від суми кредиту")]
    public decimal? PercentageRate { get; set; }
        
    [Display(Name = "Тип послуги")]
    public string ServiceType { get; set; } = string.Empty;
        
    [Display(Name = "Іконка")]
    public string IconClass { get; set; } = "fa-star";
        
    [Display(Name = "Активна")]
    public bool IsActive { get; set; } = true;

    public virtual ICollection<CustomerService> CustomerServices { get; set; } = new List<CustomerService>();
}