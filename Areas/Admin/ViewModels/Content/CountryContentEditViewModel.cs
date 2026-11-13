using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class CountryContentEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(10)]
    public string Code { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string Capital { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string Summary { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Gdp { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Population { get; init; } = string.Empty;

    // Optional: shown on the countries list card only when filled in.
    [StringLength(100)]
    public string? BankAssets { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Currency { get; init; } = string.Empty;

    [StringLength(600)]
    [Display(Name = "Profile Lead")]
    public string? ProfileLead { get; init; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Official Name")]
    public string? OfficialName { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Region { get; init; } = "Central Asia";

    [StringLength(150)]
    [Display(Name = "Population Reference Period")]
    public string? PopulationPeriod { get; init; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "GDP Reference Period")]
    public string? GdpPeriod { get; init; } = string.Empty;

    [StringLength(150)]
    [Display(Name = "Official Languages")]
    public string? OfficialLanguages { get; init; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Country Data Source")]
    public string? DataSource { get; init; } = string.Empty;

    [StringLength(4000)]
    [Display(Name = "About the Country")]
    public string? AboutText { get; init; } = string.Empty;

    [StringLength(100)]
    public string? Area { get; init; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Neighbouring Countries")]
    public string? NeighbouringCountries { get; init; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Flag Image URL")]
    public string? FlagImageUrl { get; init; } = string.Empty;

    [Display(Name = "Upload Flag")]
    public IFormFile? FlagImageFile { get; init; }

    // "publish" when the editor pressed "Publish changes" (makes the profile visible).
    public string? SubmitAction { get; init; }

    // Shown in the status line and History box; not posted back.
    public DateTimeOffset? CreatedAtUtc { get; init; }

    public DateTimeOffset? UpdatedAtUtc { get; init; }

    // Snapshot, macro, finances, analysis, priorities, news, sources and methodology sections.
    // Every field there is optional and blank rows are expected, so the implicit [Required] that
    // non-nullable strings get is switched off for the whole block.
    [Microsoft.AspNetCore.Mvc.ModelBinding.Validation.ValidateNever]
    public CAFRI.Domain.Content.CountryProfileContent Profile { get; init; } = new();

    // CSS class for the coloured card background shown when the country has no photo.
    [StringLength(150)]
    [Display(Name = "Card Background Class")]
    public string? HeroClassName { get; init; } = string.Empty;

    public string? HeroGalleryText { get; init; }

    public IFormFile[]? HeroGalleryFiles { get; init; }

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;

    // Read-only: data from other admin sections that also appears on the country profile.
    public IReadOnlyList<CountryLinkedRowViewModel> Indicators { get; init; } = [];

    public IReadOnlyList<CountryLinkedRowViewModel> Sources { get; init; } = [];
}

public sealed class CountryLinkedRowViewModel
{
    public required string Title { get; init; }

    public required string Detail { get; init; }

    public bool IsPublished { get; init; }
}
