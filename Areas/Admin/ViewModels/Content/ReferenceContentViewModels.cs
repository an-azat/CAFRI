using System.ComponentModel.DataAnnotations;
using CAFRI.Domain.Content;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class CategoryIndexViewModel
{
    public required IReadOnlyList<ContentCategory> Items { get; init; }
    public required IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; }
    public string? Search { get; init; }
    public string? Scope { get; init; }
    public string? Status { get; init; }
}

public sealed class SourceIndexViewModel
{
    public required IReadOnlyList<ContentSource> Items { get; init; }
    public required IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; }
    public required IReadOnlyList<string> AvailableTypes { get; init; }
    public required IReadOnlyList<string> AvailableCountries { get; init; }
    public string? Search { get; init; }
    public string? Type { get; init; }
    public string? Country { get; init; }
    public string? Status { get; init; }
}

public sealed class IndicatorIndexViewModel
{
    public required IReadOnlyList<CountryIndicator> Items { get; init; }
    public required IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; }
    public required IReadOnlyList<string> AvailableCategories { get; init; }
    public required IReadOnlyList<string> AvailableCountries { get; init; }
    public string? Search { get; init; }
    public string? Category { get; init; }
    public string? Country { get; init; }
    public string? Status { get; init; }
}

public sealed class ContentCategoryEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(160)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(180)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string Description { get; init; } = string.Empty;

    [Required, StringLength(80)]
    public string Scope { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;
}

public sealed class ContentSourceEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(220)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(180)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string SourceType { get; init; } = string.Empty;

    [Required, StringLength(10)]
    public string CountryCode { get; init; } = string.Empty;

    [Required, StringLength(500)]
    public string Url { get; init; } = string.Empty;

    [Required, StringLength(1200)]
    public string Summary { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;
}

public sealed class CountryIndicatorEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(10)]
    public string CountryCode { get; init; } = string.Empty;

    [Required, StringLength(180)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(180)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string Category { get; init; } = string.Empty;

    [Required, StringLength(40)]
    public string Unit { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string LatestValue { get; init; } = string.Empty;

    [Required, StringLength(60)]
    public string YearLabel { get; init; } = string.Empty;

    [Required, StringLength(220)]
    public string SourceName { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string Notes { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;
}
