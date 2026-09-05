namespace CAFRI.ViewModels.Countries;

public sealed class CountriesPageViewModel
{
    public string Title { get; init; } = "Countries";

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<CountryStatsCardViewModel> StatsCards { get; init; } = [];

    public IReadOnlyList<CountryOverviewCardViewModel> Countries { get; init; } = [];

    public IReadOnlyList<CountryCompareFeatureViewModel> CompareFeatures { get; init; } = [];

    public IReadOnlyList<string> GdpGrowthYears { get; init; } = [];

    public string SelectedGdpGrowthYear { get; init; } = string.Empty;
}
