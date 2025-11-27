using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Введіть повне ім'я")]
    [Display(Name = "Повне ім'я")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть email")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть телефон")]
    [Phone(ErrorMessage = "Невірний формат телефону")]
    [Display(Name = "Телефон")]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть пароль")]
    [StringLength(100, ErrorMessage = "Пароль має містити мінімум {2} символів", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження паролю")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
}