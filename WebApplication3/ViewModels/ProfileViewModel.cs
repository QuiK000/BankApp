using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Введіть повне ім'я")]
    [Display(Name = "Повне ім'я")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть email")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Невірний формат телефону")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Дата народження")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Адреса")]
    public string? Address { get; set; }

    [Display(Name = "ІПН")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "ІПН має містити 10 цифр")]
    public string? TaxNumber { get; set; }

    [Display(Name = "Аватар")]
    public string? AvatarUrl { get; set; }

    [Display(Name = "Дата реєстрації")]
    public DateTime RegistrationDate { get; set; }
}