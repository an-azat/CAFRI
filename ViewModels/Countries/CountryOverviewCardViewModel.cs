using CAFRI.ViewModels.Map;

namespace CAFRI.ViewModels.Countries;

public sealed class CountryOverviewCardViewModel
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public required string Capital { get; init; }

    public required string Summary { get; init; }

    public required string Gdp { get; init; }

    public required string Population { get; init; }

    public required string BankAssets { get; init; }

    public required string Slug { get; init; }

    public required string HeroClassName { get; init; }

    public string HeroImageUrl { get; init; } = string.Empty;

    public string IndicatorLabel { get; init; } = "Indicator";

    public string IndicatorValue { get; init; } = "No data";

    public IReadOnlyList<CountryMetricViewModel> Metrics { get; init; } = [];

    public IReadOnlyList<MapCountryIndicatorValueViewModel> Indicators { get; init; } = [];

    public string AccentClass => Code switch
    {
        "KZ" => "country-badge--kz",
        "UZ" => "country-badge--uz",
        "KG" => "country-badge--kg",
        "TJ" => "country-badge--tj",
        "TM" => "country-badge--tm",
        _ => "country-badge--default"
    };

    public string Url => $"/countries/{Slug}";
}
