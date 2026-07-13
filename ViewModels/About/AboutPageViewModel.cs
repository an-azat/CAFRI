namespace CAFRI.ViewModels.About;

public sealed class AboutPageViewModel
{
    public string Title { get; init; } = "About CAFRI";

    public string Description { get; init; } = string.Empty;

    public string DescriptionSecondary { get; init; } = string.Empty;

    public IReadOnlyList<AboutFeatureItemViewModel> HeroFeatures { get; init; } = [];

    public required AboutSectionCardViewModel MissionCard { get; init; }

    public IReadOnlyList<AboutSectionCardViewModel> CoverageAreas { get; init; } = [];

    public IReadOnlyList<string> GeographicCountries { get; init; } = [];

    public IReadOnlyList<string> CountryBadges { get; init; } = [];

    public IReadOnlyList<AboutSectionCardViewModel> Sources { get; init; } = [];

    public IReadOnlyList<AboutWorkflowStepViewModel> WorkflowSteps { get; init; } = [];

    public IReadOnlyList<AboutAudienceCardViewModel> Audiences { get; init; } = [];

    public IReadOnlyList<string> WhyItMattersPoints { get; init; } = [];
}
