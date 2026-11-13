namespace CAFRI.ViewModels.Countries;

using CAFRI.Domain.Content;
using CAFRI.ViewModels.Shared;

public sealed class CountryProfilePageViewModel
{
    public required CountryOverviewCardViewModel Country { get; init; }

    // Header: official name (or the short name), region, flag and lead paragraph.
    public required string Title { get; init; }

    public string Region { get; init; } = string.Empty;

    public string FlagImageUrl { get; init; } = string.Empty;

    public required string HeroDescription { get; init; }

    // "UPDATED · 04 NOV 2026" label in the header.
    public string UpdatedLabel { get; init; } = string.Empty;

    // Population, GDP, currency and capital, each with its reference period or detail line.
    public IReadOnlyList<CountryHeroFactViewModel> HeroFacts { get; init; } = [];

    public string DataSource { get; init; } = string.Empty;

    // First gallery photo, used as the header background.
    public string HeroBackgroundUrl { get; init; } = string.Empty;

    public IReadOnlyList<ContentImageItemViewModel> HeroGallery { get; init; } = [];

    public IReadOnlyList<string> AboutParagraphs { get; init; } = [];

    public IReadOnlyList<CountryHeroFactViewModel> AboutFacts { get; init; } = [];

    // Editorial sections: macro signals, public finances, analysis, priorities, news, methodology.
    public CountryProfileContent Profile { get; init; } = new();

    // Economic snapshot cards: the editor's KPIs, or the latest CountryIndicators values.
    public IReadOnlyList<CountryProfileKpi> SnapshotKpis { get; init; } = [];

    // Real GDP growth by year: the editor's series, or the "Real GDP Growth" indicator rows.
    public IReadOnlyList<CountryProfileGdpPoint> GdpHistory { get; init; } = [];

    // The editor's data sources, or the official sources registered in the Sources section.
    public IReadOnlyList<CountryProfileSource> DataSources { get; init; } = [];

    // CAFRI's own coverage of the country: published Intelligence and Publications.
    public IReadOnlyList<CountrySidebarLinkItemViewModel> LatestUpdates { get; init; } = [];

    public IReadOnlyList<CountrySidebarLinkItemViewModel> RelatedPublications { get; init; } = [];
}

public sealed class CountryHeroFactViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public string Note { get; init; } = string.Empty;
}
