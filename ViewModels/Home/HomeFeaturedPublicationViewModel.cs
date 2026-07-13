namespace CAFRI.ViewModels.Home;

public sealed class HomeFeaturedPublicationViewModel
{
    public string Eyebrow { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Subtitle { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string ReadOnlineUrl { get; init; } = "#";
    public string DownloadUrl { get; init; } = "#";
    public IReadOnlyList<HomeFeaturedMetaItemViewModel> MetaItems { get; init; } = [];
}
