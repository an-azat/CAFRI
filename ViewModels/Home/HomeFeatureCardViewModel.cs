namespace CAFRI.ViewModels.Home;

public sealed class HomeFeatureCardViewModel
{
    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Url { get; init; } = "#";

    public string CssClassName { get; init; } = string.Empty;
}
