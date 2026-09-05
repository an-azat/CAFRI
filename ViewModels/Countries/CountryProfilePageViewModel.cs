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

    // Classifies the free-text credit-rating letter grade (S&amp;P/Fitch-style: AAA..D) into a
    // CSS tone so the badge color reflects how strong the rating actually is, instead of a
    // single hardcoded "positive" color for every country regardless of rating.
    public string RiskRatingTone
    {
        get
        {
            var grade = RiskRating.TrimStart().ToUpperInvariant();
            if (grade.StartsWith("AAA", StringComparison.Ordinal) ||
                grade.StartsWith("AA", StringComparison.Ordinal) ||
                (grade.StartsWith("A", StringComparison.Ordinal) && !grade.StartsWith("AB", StringComparison.Ordinal)))
            {
                return "positive";
            }

            if (grade.StartsWith("BBB", StringComparison.Ordinal))
            {
                return "positive";
            }

            if (grade.StartsWith("BB", StringComparison.Ordinal))
            {
                return "neutral";
            }

            if (grade.StartsWith("B", StringComparison.Ordinal))
            {
                return "caution";
            }

            if (grade.StartsWith("CCC", StringComparison.Ordinal) ||
                grade.StartsWith("CC", StringComparison.Ordinal) ||
                grade.StartsWith("C", StringComparison.Ordinal) ||
                grade.StartsWith("D", StringComparison.Ordinal))
            {
                return "negative";
            }

            return "neutral";
        }
    }
}
