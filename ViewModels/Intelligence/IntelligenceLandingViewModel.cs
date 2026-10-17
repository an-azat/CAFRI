namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceLandingViewModel
{
    public int TotalPublished { get; init; }

    public IntelligenceItemCardViewModel? LeadItem { get; init; }

    public IReadOnlyList<IntelligenceItemCardViewModel> SecondaryItems { get; init; } = [];

    public IReadOnlyList<IntelligenceCountryDigestViewModel> Countries { get; init; } = [];

    public IReadOnlyList<IntelligenceItemCardViewModel> TrendingItems { get; init; } = [];
}

public sealed class IntelligenceCountryDigestViewModel
{
    public required string Code { get; init; }

    public required string Name { get; init; }

    public int TotalItems { get; init; }

    public IReadOnlyList<IntelligenceItemCardViewModel> LatestItems { get; init; } = [];

    public string FlagIconUrl => CAFRI.ViewModels.Countries.CountryFlags.IconUrl(Code);

    public string ArchiveUrl => $"/intelligence/archive?country={Uri.EscapeDataString(Name)}";

    public string AccentClass => Code switch
    {
        "KZ" => "intel-country__code--kz",
        "UZ" => "intel-country__code--uz",
        "KG" => "intel-country__code--kg",
        "TJ" => "intel-country__code--tj",
        "TM" => "intel-country__code--tm",
        _ => string.Empty
    };
}
