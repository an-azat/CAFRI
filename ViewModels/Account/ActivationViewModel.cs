using System.ComponentModel.DataAnnotations;

namespace CAFRI.ViewModels.Account;

public sealed class ActivationViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Password")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 10)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Confirm password")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
