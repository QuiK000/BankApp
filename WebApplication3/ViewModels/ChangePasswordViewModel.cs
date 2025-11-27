using System.ComponentModel.DataAnnotations;

namespace WebApplication3.ViewModels;

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Введіть поточний пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Поточний пароль")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введіть новий пароль")]
    [StringLength(100, ErrorMessage = "Пароль має містити мінімум {2} символів", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Новий пароль")]
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Підтвердження нового паролю")]
    [Compare("NewPassword", ErrorMessage = "Паролі не співпадають")]
    public string ConfirmPassword { get; set; } = string.Empty;
}