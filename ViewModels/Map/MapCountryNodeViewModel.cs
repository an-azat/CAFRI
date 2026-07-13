namespace CAFRI.ViewModels.Map;

public sealed class MapCountryNodeViewModel
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public required string Capital { get; init; }

    public required string Gdp { get; init; }

    public required string Population { get; init; }

    public required string BankAssets { get; init; }

    public required string IndicatorLabel { get; init; }

    public required string IndicatorValue { get; init; }

    public required string Summary { get; init; }

    public required string AccentClass { get; init; }

    public required string PathData { get; init; }

    public required decimal LabelX { get; init; }

    public required decimal LabelY { get; init; }

    public required decimal CapitalX { get; init; }

    public required decimal CapitalY { get; init; }

    public required decimal CapitalLabelX { get; init; }

    public required decimal CapitalLabelY { get; init; }

    public IReadOnlyList<MapCountryIndicatorValueViewModel> Indicators { get; init; } = [];
}
