namespace CAFRI.ViewModels.Countries;

public sealed class CountrySidebarCardViewModel
{
    public required string Title { get; init; }

    public IReadOnlyList<CountrySidebarLinkItemViewModel> Items { get; init; } = [];

    // Shown instead of the list when there are no items.
    public string? EmptyText { get; init; }

    public string? LinkLabel { get; init; }

    public string? LinkUrl { get; init; }
}
