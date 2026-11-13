namespace CAFRI.ViewModels.Countries;

public sealed class CountrySidebarLinkItemViewModel
{
    public required string Title { get; init; }

    public required string Meta { get; init; }

    public string? Url { get; init; }
}
