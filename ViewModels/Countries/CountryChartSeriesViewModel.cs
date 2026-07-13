namespace CAFRI.ViewModels.Countries;

public sealed class CountryChartSeriesViewModel
{
    public required string Title { get; init; }

    public required string ValueSuffix { get; init; }

    public IReadOnlyList<string> Labels { get; init; } = [];

    public IReadOnlyList<decimal> Values { get; init; } = [];
}
