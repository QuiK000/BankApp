using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models;

public class Customer
{
    public int Id { get; set; }
        
    [Required(ErrorMessage = "Введіть ім'я")]
    [Display(Name = "Повне ім'я")]
    public string FullName { get; set; } = string.Empty;
        
    [Required(ErrorMessage = "Введіть телефон")]
    [Phone(ErrorMessage = "Невірний формат телефону")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;
        
    [Required(ErrorMessage = "Введіть email")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;
        
    [Required(ErrorMessage = "Введіть ІПН")]
    [Display(Name = "ІПН")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "ІПН має містити 10 цифр")]
    public string TaxNumber { get; set; } = string.Empty;
        
    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }
        
    [Display(Name = "Адреса")]
    public string Address { get; set; } = string.Empty;
        
    [Display(Name = "Дата реєстрації")]
    public DateTime RegistrationDate { get; set; } = DateTime.Now;
        
    [Display(Name = "Статус")]
    public string Status { get; set; } = "Активний";

    public virtual ICollection<CustomerCredit> CustomerCredits { get; set; } = new List<CustomerCredit>();

    public virtual ICollection<CustomerService> CustomerServices { get; set; } = new List<CustomerService>();
}
