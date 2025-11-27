using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace WebApplication3.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [Display(Name = "Повне ім'я")]
    public string FullName { get; set; } = string.Empty;
        
    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }
        
    [Display(Name = "Адреса")]
    public string? Address { get; set; }
        
    [Display(Name = "ІПН")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "ІПН має містити 10 цифр")]
    public string? TaxNumber { get; set; }
        
    [Display(Name = "Дата реєстрації")]
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
    [Display(Name = "Аватар")]
    public string? AvatarUrl { get; set; }
    
    public virtual ICollection<CreditApplication> CreditApplications { get; set; } = new List<CreditApplication>();
}