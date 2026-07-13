namespace CAFRI.ViewModels.Countries;

public sealed class CountryExploreLinkViewModel
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public string Url { get; init; } = "#";
}
