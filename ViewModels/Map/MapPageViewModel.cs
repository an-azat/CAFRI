namespace CAFRI.ViewModels.Map;

public sealed class MapPageViewModel
{
    public string Title { get; init; } = "Interactive Map";

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<CAFRI.ViewModels.Countries.CountryStatsCardViewModel> SummaryCards { get; init; } = [];

    public IReadOnlyList<MapLayerOptionViewModel> DataCategories { get; init; } = [];

    public IReadOnlyList<string> MapStyles { get; init; } = [];

    public string SelectedMapStyle { get; init; } = "Dark";

    public IReadOnlyList<MapLayerOptionViewModel> Overlays { get; init; } = [];

    public IReadOnlyList<string> IndicatorOptions { get; init; } = [];

    public string SelectedIndicator { get; init; } = "GDP Growth (2025)";

    public IReadOnlyList<MapLegendItemViewModel> LegendItems { get; init; } = [];

    public IReadOnlyList<MapCountryNodeViewModel> Countries { get; init; } = [];

    public IReadOnlyDictionary<string, MapCountryNodeViewModel> CountryLookup { get; init; } = new Dictionary<string, MapCountryNodeViewModel>(StringComparer.OrdinalIgnoreCase);
}
