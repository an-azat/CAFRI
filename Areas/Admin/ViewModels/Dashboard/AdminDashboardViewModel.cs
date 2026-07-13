namespace CAFRI.Areas.Admin.ViewModels.Dashboard;

public sealed class AdminDashboardViewModel
{
    public required IReadOnlyList<AdminMetricCardViewModel> Metrics { get; init; }

    public required IReadOnlyList<AdminRecentContentItemViewModel> RecentContent { get; init; }
}

public sealed class AdminMetricCardViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public required string ChangeLabel { get; init; }

    public required string Accent { get; init; }
}

public sealed class AdminRecentContentItemViewModel
{
    public required string Title { get; init; }

    public required string ContentType { get; init; }

    public required string CountryOrCoverage { get; init; }

    public required string TopicOrCategory { get; init; }

    public required string Status { get; init; }

    public required string UpdatedLabel { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }

    public string? EditUrl { get; init; }

    public string? PreviewUrl { get; init; }
}
