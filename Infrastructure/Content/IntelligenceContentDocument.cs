using CAFRI.ViewModels.Intelligence;

namespace CAFRI.Infrastructure.Content;

public sealed class IntelligenceContentDocument
{
    public string Description { get; init; } = string.Empty;

    public string SearchPlaceholder { get; init; } = "Search intelligence...";

    public IReadOnlyList<string> CountrySelectOptions { get; init; } = [];

    public IReadOnlyList<string> CategorySelectOptions { get; init; } = [];

    public IReadOnlyList<string> InstitutionSelectOptions { get; init; } = [];

    public IReadOnlyList<IntelligenceFilterOptionViewModel> SidebarCountries { get; init; } = [];

    public IReadOnlyList<IntelligenceFilterOptionViewModel> SidebarCategories { get; init; } = [];

    public IReadOnlyList<string> DateRangeOptions { get; init; } = [];

    public IReadOnlyList<IntelligenceItemCardViewModel> Items { get; init; } = [];

    public int ShowingFrom { get; init; }

    public int ShowingTo { get; init; }

    public int TotalResults { get; init; }

    public int CurrentPage { get; init; }

    public int TotalPages { get; init; }

    public string SortBy { get; init; } = "Newest";

    public IReadOnlyList<string> ProfessionalAccessBenefits { get; init; } = [];

    public IReadOnlyList<IntelligenceDetailsViewModel> Details { get; init; } = [];
}
