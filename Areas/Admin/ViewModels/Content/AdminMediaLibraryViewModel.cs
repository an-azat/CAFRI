namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class AdminMediaLibraryViewModel
{
    public IReadOnlyList<AdminMediaAssetViewModel> Items { get; init; } = [];

    public IReadOnlyList<AdminSummaryCardViewModel> SummaryCards { get; init; } = [];

    public string Search { get; init; } = string.Empty;

    public string Section { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableSections { get; init; } = [];
}

public sealed class AdminMediaAssetViewModel
{
    public required string Url { get; init; }

    public required string FileName { get; init; }

    public required string Section { get; init; }

    public required string SizeLabel { get; init; }

    public required string UpdatedLabel { get; init; }

    public bool IsUsed { get; init; }

    public int UsageCount { get; init; }
}
