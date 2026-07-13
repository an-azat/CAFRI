namespace CAFRI.ViewModels.Countries;

using CAFRI.ViewModels.Shared;

public sealed class CountryProfilePageViewModel
{
    public required CountryOverviewCardViewModel Country { get; init; }

    public required string HeroDescription { get; init; }

    public required string Currency { get; init; }

    public required string RiskRating { get; init; }

    public required string RiskOutlook { get; init; }

    public required string DoingBusinessRank { get; init; }

    public required string DoingBusinessSource { get; init; }

    public required string OverviewText { get; init; }

    public IReadOnlyList<ContentImageItemViewModel> HeroGallery { get; set; } = [];

    public IReadOnlyList<CountryMetricViewModel> HeroIndicators { get; init; } = [];

    public IReadOnlyList<CountryKeyIndicatorRowViewModel> KeyIndicators { get; init; } = [];

    public IReadOnlyList<CountryTabViewModel> Tabs { get; init; } = [];

    public IReadOnlyList<CountryHighlightViewModel> Highlights { get; init; } = [];

    public IReadOnlyList<CountrySnapshotCardViewModel> MacroSummaryCards { get; init; } = [];

    public IReadOnlyList<CountryMacroIndicatorRowViewModel> MacroIndicatorRows { get; init; } = [];

    public required CountryChartSeriesViewModel GdpGrowthChart { get; init; }

    public required CountryChartSeriesViewModel InflationChart { get; init; }

    public IReadOnlyList<CountrySnapshotCardViewModel> FinancialSectorSnapshot { get; init; } = [];

    public required CountryChartSeriesViewModel BankingAssetsChart { get; init; }

    public IReadOnlyList<CountryTopBankViewModel> TopBanks { get; init; } = [];

    public IReadOnlyList<CountrySidebarLinkItemViewModel> LatestUpdates { get; init; } = [];

    public IReadOnlyList<CountrySidebarLinkItemViewModel> RelatedPublications { get; init; } = [];

    public IReadOnlyList<CountryInstitutionViewModel> KeyInstitutions { get; init; } = [];

    public IReadOnlyList<CountryExploreLinkViewModel> ExploreLinks { get; init; } = [];
}
