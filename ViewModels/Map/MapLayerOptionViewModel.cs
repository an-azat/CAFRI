namespace CAFRI.ViewModels.Map;

public sealed class MapLayerOptionViewModel
{
    public required string Icon { get; init; }

    public required string Label { get; init; }

    public bool IsSelected { get; init; }
}
