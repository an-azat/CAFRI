using CAFRI.Domain.Content;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class PublicationIndexViewModel
{
    public required IReadOnlyList<PublicationContentItem> Items { get; init; }

    public required IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; }

    public required IReadOnlyList<string> AvailableTypes { get; init; }

    public required IReadOnlyList<string> AvailableCountries { get; init; }

    public required IReadOnlyList<string> AvailableTopics { get; init; }

    public string? Search { get; init; }

    public string? Type { get; init; }

    public string? Country { get; init; }

    public string? Topic { get; init; }

    public string? Status { get; init; }
}

public sealed class IntelligenceIndexViewModel
{
    public required IReadOnlyList<IntelligenceContentItem> Items { get; init; }

    public required IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; }

    public required IReadOnlyList<string> AvailableCategories { get; init; }

    public required IReadOnlyList<string> AvailableCountries { get; init; }

    public string? Search { get; init; }

    public string? Category { get; init; }

    public string? Country { get; init; }

    public string? Status { get; init; }
}

public sealed class CountryIndexViewModel
{
    public required IReadOnlyList<CountryContent> Items { get; init; }

    public required IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; }

    public string? Search { get; init; }

    public string? Status { get; init; }
}

public sealed class AdminSummaryCardViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public required string Caption { get; init; }
}
