using System.ComponentModel.DataAnnotations;

namespace InstagramApp.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Логин обязателен")]
    [Display(Name = "Логин")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный адрес Email")]
    [Display(Name = "Адрес электронной почты")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Пароль обязателен")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Подтвердите пароль")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Пароли не совпадают")]
    [Display(Name = "Подтвердите пароль")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Аватар")]
    public IFormFile? AvatarFile { get; set; }

    [Display(Name = "Имя")]
    public string? Name { get; set; }

    [Display(Name = "Информация о себе")]
    public string? Bio { get; set; }

    [Display(Name = "Номер телефона")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Пол")]
    public string? Gender { get; set; }
}

public class LoginViewModel
{
    [Required(ErrorMessage = "Введите логин или Email")]
    [Display(Name = "Логин или Email")]
    public string LoginOrEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите пароль")]
    [DataType(DataType.Password)]
    [Display(Name = "Пароль")]
    public string Password { get; set; } = string.Empty;
}