using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models;

public class CustomerService
{
    public int Id { get; set; }
    
    public int CustomerId { get; set; }
    public int ServiceId { get; set; }
    
    public virtual Customer Customer { get; set; } = null!;
    public virtual Service Service { get; set; } = null!;
    
    [Display(Name = "Дата підключення")]
    public DateTime ActivationDate { get; set; } = DateTime.Now;
        
    [Display(Name = "Статус послуги")]
    public string Status { get; set; } = "Активна";
        
    [Display(Name = "Вартість")]
    public decimal TotalCost { get; set; }
        
    [Display(Name = "Примітки")]
    public string? Notes { get; set; }
}