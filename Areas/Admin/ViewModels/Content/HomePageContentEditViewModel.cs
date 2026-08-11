using System.ComponentModel.DataAnnotations;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class HomePageContentEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(300)]
    public string HeroTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string HeroLead { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string HeroPrimaryCtaLabel { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string HeroPrimaryCtaUrl { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string HeroSecondaryCtaLabel { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string HeroSecondaryCtaUrl { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string IntroTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string IntroDescription { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string PlatformSectionLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string PlatformSectionTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string PlatformSectionDescription { get; init; } = string.Empty;

    [Required]
    public string DirectionsText { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string CountriesSectionLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string CountriesSectionTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string CountriesSectionDescription { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string CountriesVisualTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string CountriesVisualDescription { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string FeaturesSectionLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string FeaturesSectionTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string FeaturesSectionDescription { get; init; } = string.Empty;

    [Required]
    public string FeatureCardsText { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string LatestSectionLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string LatestSectionTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string LatestSectionDescription { get; init; } = string.Empty;

    [Required, StringLength(160)]
    public string FeaturedPublicationEyebrow { get; init; } = string.Empty;

    [Required, StringLength(160)]
    public string LatestFeaturedLinkLabel { get; init; } = string.Empty;

    [Required, StringLength(160)]
    public string LatestIntelligenceLinkLabel { get; init; } = string.Empty;

    public Guid? FeaturedPublicationId { get; init; }

    public IReadOnlyList<AdminPublicationOptionViewModel> AvailablePublications { get; init; } = [];

    [Required, StringLength(300)]
    public string AboutTitle { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string AboutDescription { get; init; } = string.Empty;

    [Required]
    public string CoverageAreasText { get; init; } = string.Empty;
}
