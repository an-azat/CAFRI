namespace CAFRI.ViewModels.Countries;

public sealed class CountrySidebarCardViewModel
{
    public required string Title { get; init; }

    public IReadOnlyList<CountrySidebarLinkItemViewModel> Items { get; init; } = [];

    public string? LinkLabel { get; init; }

    public string? LinkUrl { get; init; }
}
