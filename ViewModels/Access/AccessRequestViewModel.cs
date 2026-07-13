using System.ComponentModel.DataAnnotations;

namespace CAFRI.ViewModels.Access;

public sealed class AccessRequestViewModel
{
    [Required]
    [Display(Name = "Full name")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Organization")]
    [StringLength(200)]
    public string Organization { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Position")]
    [StringLength(150)]
    public string Position { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [Display(Name = "Business email")]
    [StringLength(256)]
    public string BusinessEmail { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Country")]
    [StringLength(100)]
    public string Country { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Purpose of access")]
    [StringLength(2000, MinimumLength = 20)]
    public string PurposeOfAccess { get; set; } = string.Empty;
}
