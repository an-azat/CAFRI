namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceSidebarLinkViewModel
{
    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public string? Url { get; init; }
}
