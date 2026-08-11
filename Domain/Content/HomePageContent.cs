namespace CAFRI.Domain.Content;

public sealed class HomePageContent
{
    public Guid Id { get; set; }

    public string HeroTitle { get; set; } = string.Empty;

    public string HeroLead { get; set; } = string.Empty;

    public string HeroPrimaryCtaLabel { get; set; } = string.Empty;

    public string HeroPrimaryCtaUrl { get; set; } = string.Empty;

    public string HeroSecondaryCtaLabel { get; set; } = string.Empty;

    public string HeroSecondaryCtaUrl { get; set; } = string.Empty;

    public string IntroTitle { get; set; } = string.Empty;

    public string IntroDescription { get; set; } = string.Empty;

    public string PlatformSectionLabel { get; set; } = string.Empty;

    public string PlatformSectionTitle { get; set; } = string.Empty;

    public string PlatformSectionDescription { get; set; } = string.Empty;

    public string DirectionsText { get; set; } = string.Empty;

    public string CountriesSectionLabel { get; set; } = string.Empty;

    public string CountriesSectionTitle { get; set; } = string.Empty;

    public string CountriesSectionDescription { get; set; } = string.Empty;

    public string CountriesVisualTitle { get; set; } = string.Empty;

    public string CountriesVisualDescription { get; set; } = string.Empty;

    public string FeaturesSectionLabel { get; set; } = string.Empty;

    public string FeaturesSectionTitle { get; set; } = string.Empty;

    public string FeaturesSectionDescription { get; set; } = string.Empty;

    public string FeatureCardsText { get; set; } = string.Empty;

    public string LatestSectionLabel { get; set; } = string.Empty;

    public string LatestSectionTitle { get; set; } = string.Empty;

    public string LatestSectionDescription { get; set; } = string.Empty;

    public string FeaturedPublicationEyebrow { get; set; } = string.Empty;

    public string LatestFeaturedLinkLabel { get; set; } = string.Empty;

    public string LatestIntelligenceLinkLabel { get; set; } = string.Empty;

    public Guid? FeaturedPublicationId { get; set; }

    public string AboutTitle { get; set; } = string.Empty;

    public string AboutDescription { get; set; } = string.Empty;

    public string CoverageAreasText { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
