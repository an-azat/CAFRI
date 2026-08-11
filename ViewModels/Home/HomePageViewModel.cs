namespace CAFRI.ViewModels.Home;

public sealed class HomePageViewModel
{
    public string HeroTitle { get; init; } = string.Empty;
    public string HeroLead { get; init; } = string.Empty;
    public string HeroPrimaryCtaLabel { get; init; } = string.Empty;
    public string HeroPrimaryCtaUrl { get; init; } = "#";
    public string HeroSecondaryCtaLabel { get; init; } = string.Empty;
    public string HeroSecondaryCtaUrl { get; init; } = "#";
    public string IntroTitle { get; init; } = string.Empty;
    public string IntroDescription { get; init; } = string.Empty;
    public string PlatformSectionLabel { get; init; } = string.Empty;
    public string PlatformSectionTitle { get; init; } = string.Empty;
    public string PlatformSectionDescription { get; init; } = string.Empty;
    public IReadOnlyList<HomeDirectionCardViewModel> Directions { get; init; } = [];
    public string CountriesSectionLabel { get; init; } = string.Empty;
    public string CountriesSectionTitle { get; init; } = string.Empty;
    public string CountriesSectionDescription { get; init; } = string.Empty;
    public string CountriesVisualTitle { get; init; } = string.Empty;
    public string CountriesVisualDescription { get; init; } = string.Empty;
    public string FeaturesSectionLabel { get; init; } = string.Empty;
    public string FeaturesSectionTitle { get; init; } = string.Empty;
    public string FeaturesSectionDescription { get; init; } = string.Empty;
    public IReadOnlyList<HomeFeatureCardViewModel> FeatureCards { get; init; } = [];
    public string LatestSectionLabel { get; init; } = string.Empty;
    public string LatestSectionTitle { get; init; } = string.Empty;
    public string LatestSectionDescription { get; init; } = string.Empty;
    public string FeaturedPublicationEyebrow { get; init; } = string.Empty;
    public string LatestFeaturedLinkLabel { get; init; } = string.Empty;
    public string LatestIntelligenceLinkLabel { get; init; } = string.Empty;
    public string AboutTitle { get; init; } = string.Empty;
    public string AboutDescription { get; init; } = string.Empty;
    public IReadOnlyList<HomeIntelligenceCardViewModel> LatestIntelligence { get; init; } = [];
    public HomeFeaturedPublicationViewModel FeaturedPublication { get; init; } = new();
    public IReadOnlyList<HomeCoverageAreaItemViewModel> CoverageAreas { get; init; } = [];
    public IReadOnlyList<CAFRI.ViewModels.Countries.CountryOverviewCardViewModel> Countries { get; init; } = [];
}
