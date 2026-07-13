namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceOfficialDocumentViewModel
{
    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public required string Meta { get; init; }

    public string DownloadUrl { get; init; } = "#";
}
