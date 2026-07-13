namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligencePageViewModel
{
    public string Title { get; init; } = "Intelligence";

    public string Description { get; init; } = string.Empty;

    public string SearchPlaceholder { get; init; } = "Search intelligence...";

    public IReadOnlyList<string> CountrySelectOptions { get; init; } = [];

    public IReadOnlyList<string> CategorySelectOptions { get; init; } = [];

    public IReadOnlyList<string> InstitutionSelectOptions { get; init; } = [];

    public IReadOnlyList<IntelligenceFilterOptionViewModel> SidebarCountries { get; init; } = [];

    public IReadOnlyList<IntelligenceFilterOptionViewModel> SidebarCategories { get; init; } = [];

    public IReadOnlyList<string> DateRangeOptions { get; init; } = [];

    public IReadOnlyList<IntelligenceItemCardViewModel> Items { get; init; } = [];

    public IReadOnlyList<IntelligenceSummaryMetricViewModel> SummaryMetrics { get; init; } = [];

    public string? SearchTerm { get; init; }

    public string CountryFilter { get; init; } = "All Countries";

    public string CategoryFilter { get; init; } = "All Categories";

    public string InstitutionFilter { get; init; } = "All Institutions";

    public string DateRangeFilter { get; init; } = "Any time";

    public int ShowingFrom { get; init; }

    public int ShowingTo { get; init; }

    public int TotalResults { get; init; }

    public int CurrentPage { get; init; }

    public int TotalPages { get; init; }

    public int PageSize { get; init; }

    public string SortBy { get; init; } = "Newest";

    public IReadOnlyList<string> ProfessionalAccessBenefits { get; init; } = [];
}
