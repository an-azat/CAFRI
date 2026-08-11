namespace CAFRI.ViewModels.Home;

public sealed class HomeDirectionCardViewModel
{
    public string Number { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Url { get; init; } = "#";

    public string LinkLabel { get; init; } = string.Empty;

    public bool IsWide { get; init; }
}
