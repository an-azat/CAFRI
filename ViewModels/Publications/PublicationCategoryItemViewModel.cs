namespace CAFRI.ViewModels.Publications;

public sealed class PublicationCategoryItemViewModel
{
    public required string Icon { get; init; }

    public required string Label { get; init; }

    public required int Count { get; init; }

    public string Url { get; init; } = "/publications";

    public bool IsActive { get; init; }
}
