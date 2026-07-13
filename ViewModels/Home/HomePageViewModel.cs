namespace CAFRI.ViewModels.Home;

public sealed class HomePageViewModel
{
    public IReadOnlyList<HomeIntelligenceCardViewModel> LatestIntelligence { get; init; } = [];
    public HomeFeaturedPublicationViewModel FeaturedPublication { get; init; } = new();
    public IReadOnlyList<HomeCoverageAreaItemViewModel> CoverageAreas { get; init; } = [];
    public IReadOnlyList<CAFRI.ViewModels.Countries.CountryOverviewCardViewModel> Countries { get; init; } = [];
}
