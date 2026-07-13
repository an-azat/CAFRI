namespace CAFRI.Areas.Admin.ViewModels.AccessRequests;

public sealed class AccessRequestIndexViewModel
{
    public required IReadOnlyList<AccessRequestReviewViewModel> Items { get; init; }

    public required IReadOnlyList<AdminAccessSummaryCardViewModel> SummaryCards { get; init; }

    public string? Search { get; init; }

    public string? Status { get; init; }
}

public sealed class AdminAccessSummaryCardViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public required string Caption { get; init; }
}
