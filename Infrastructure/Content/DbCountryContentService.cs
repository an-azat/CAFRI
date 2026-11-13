using CAFRI.Application.Abstractions.Services;
using CAFRI.ViewModels.Shared;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Countries;
using CAFRI.ViewModels.Map;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public sealed class DbCountryContentService : ICountryContentService
{
    private readonly AppDbContext _dbContext;

    public DbCountryContentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CountriesPageViewModel> GetIndexPageAsync(CancellationToken cancellationToken = default)
    {
        var countryRows = await _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var indicators = await _dbContext.CountryIndicators
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        var selectedIndicatorName = indicators
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        var gdpGrowthYears = indicators
            .Where(x => string.Equals(x.Name, "Real GDP Growth", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.YearLabel)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var selectedGdpGrowthYear = gdpGrowthYears.LastOrDefault() ?? string.Empty;

        var countries = countryRows
            .Select(x =>
            {
                var countryIndicators = indicators
                    .Where(item => string.Equals(item.CountryCode, x.Code, StringComparison.OrdinalIgnoreCase))
                    .Select(item => new MapCountryIndicatorValueViewModel
                    {
                        Name = item.Name,
                        Label = $"{item.Name} {item.YearLabel}".Trim(),
                        Value = string.IsNullOrWhiteSpace(item.Unit) ? item.LatestValue : $"{item.LatestValue} {item.Unit}",
                        Category = item.Category,
                        Unit = item.Unit,
                        YearLabel = item.YearLabel,
                        SourceName = item.SourceName
                    })
                    .ToList();

                var primaryIndicator = indicators.FirstOrDefault(item =>
                    string.Equals(item.CountryCode, x.Code, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(item.Name, selectedIndicatorName, StringComparison.OrdinalIgnoreCase));

                return new CountryOverviewCardViewModel
                {
                    Code = x.Code,
                    Name = x.Name,
                    Capital = x.Capital,
                    Summary = x.Summary,
                    Gdp = x.Gdp,
                    Population = x.Population,
                    BankAssets = x.BankAssets,
                    Slug = x.Slug,
                    HeroClassName = x.HeroClassName,
                    HeroImageUrl = ContentImageTextSerializer.Parse(x.HeroGalleryJson)
                        .Select(item => item.ImageUrl)
                        .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url)) ?? string.Empty,
                    IndicatorLabel = primaryIndicator is null ? "Indicator" : $"{primaryIndicator.Name} {primaryIndicator.YearLabel}".Trim(),
                    IndicatorValue = primaryIndicator?.LatestValue ?? "No data",
                    Indicators = countryIndicators,
                    Metrics =
                    [
                        new() { Icon = "chart", Label = "GDP (2025)", Value = x.Gdp },
                        new() { Icon = "people", Label = "Population", Value = x.Population },
                        new() { Icon = "bank", Label = "Bank Assets", Value = x.BankAssets }
                    ]
                };
            })
            .ToList();

        return new CountriesPageViewModel
        {
            Description = "Country intelligence profiles covering financial regulation, banking sector, macroeconomic indicators, trade, sanctions exposure, and institutional developments across Central Asia.",
            Countries = countries,
            GdpGrowthYears = gdpGrowthYears,
            SelectedGdpGrowthYear = selectedGdpGrowthYear
        };
    }

    public async Task<CountryProfilePageViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.CountryContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsPublished && x.Slug == slug, cancellationToken);

        return entity is null ? null : await BuildProfileAsync(entity, cancellationToken);
    }

    // The profile is built only from data the admin manages: the country fields, the
    // CountryIndicators table, published Intelligence and Publications for the country, and the
    // official sources registered for it.
    private async Task<CountryProfilePageViewModel> BuildProfileAsync(
        Domain.Content.CountryContent entity,
        CancellationToken cancellationToken)
    {
        var code = entity.Code;
        var heroGallery = ContentImageTextSerializer.Parse(entity.HeroGalleryJson);

        var indicators = await _dbContext.CountryIndicators
            .AsNoTracking()
            .Where(x => x.IsPublished && x.CountryCode == code)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
        var indicatorGroups = indicators
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var profile = CountryProfileContentSerializer.Parse(entity.ProfileJson);

        // Economic snapshot: the four standard cards always come first. Each is filled from the
        // editor's KPI with a matching label, else from the latest matching indicator, else it
        // shows "No data yet". Any other editor KPIs follow.
        var latestIndicators = indicatorGroups
            .Select(group => group.OrderByDescending(x => x.YearLabel, StringComparer.Ordinal).First())
            .ToList();
        var snapshotKpis = new List<Domain.Content.CountryProfileKpi>();
        var usedKpis = new HashSet<Domain.Content.CountryProfileKpi>();
        foreach (var standard in Domain.Content.CountryProfileStandards.SnapshotIndicators)
        {
            var curated = profile.Kpis.FirstOrDefault(x => !usedKpis.Contains(x) && standard.Matches(x.Label));
            if (curated is not null)
            {
                usedKpis.Add(curated);
                snapshotKpis.Add(curated);
                continue;
            }

            var indicator = latestIndicators.FirstOrDefault(x => standard.Matches(x.Name));
            snapshotKpis.Add(indicator is null
                ? new Domain.Content.CountryProfileKpi { Label = standard.Label, Value = "—", Period = "No data yet" }
                : new Domain.Content.CountryProfileKpi
                {
                    Label = standard.Label,
                    Value = FormatIndicatorValue(indicator.LatestValue, indicator.Unit),
                    Period = indicator.YearLabel,
                    Source = indicator.SourceName
                });
        }

        snapshotKpis.AddRange(profile.Kpis.Where(x => !usedKpis.Contains(x)));

        // GDP history: the editor's series (with actual/estimate/forecast), or the yearly
        // "Real GDP Growth" indicator rows.
        var gdpHistory = profile.GdpHistory.Count > 0
            ? profile.GdpHistory
            : indicators
                .Where(x => string.Equals(x.Name, "Real GDP Growth", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(x.YearLabel))
                .Select(x => new Domain.Content.CountryProfileGdpPoint
                {
                    Year = x.YearLabel,
                    Value = decimal.TryParse(x.LatestValue, System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var value) ? value : null,
                    Source = x.SourceName
                })
                .Where(x => x.Value.HasValue)
                .OrderBy(x => x.Year, StringComparer.Ordinal)
                .ToList();

        var latestUpdates = (await _dbContext.IntelligenceContentItems
                .AsNoTracking()
                .Where(x => x.IsPublished && x.CountryCode == code)
                .OrderBy(x => x.PublishedAtUtc == null)
                .ThenByDescending(x => x.PublishedAtUtc)
                .Take(3)
                .Select(x => new { x.Title, x.Slug, x.Category, x.PublishedAtUtc })
                .ToListAsync(cancellationToken))
            .Select(x => new CountrySidebarLinkItemViewModel
            {
                Title = x.Title,
                Meta = JoinMeta(x.Category, PublicationMetadataFormatter.FormatShortDate(x.PublishedAtUtc)),
                Url = $"/intelligence/{x.Slug}"
            })
            .ToList();

        var relatedPublications = (await _dbContext.PublicationContentItems
                .AsNoTracking()
                .Where(x => x.WorkflowStatus == Domain.Content.PublicationWorkflowStatuses.Published &&
                            x.Visibility != Domain.Content.PublicationVisibility.Hidden &&
                            (x.CountryCode == code || x.CountryAssignments.Any(item => item.CountryCode == code)))
                .OrderBy(x => x.PublishedAtUtc == null)
                .ThenByDescending(x => x.PublishedAtUtc)
                .Take(3)
                .Select(x => new { x.Title, x.Slug, x.Type, x.PublishedAtUtc })
                .ToListAsync(cancellationToken))
            .Select(x => new CountrySidebarLinkItemViewModel
            {
                Title = x.Title,
                Meta = JoinMeta(System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(x.Type.ToLowerInvariant()), PublicationMetadataFormatter.FormatShortDate(x.PublishedAtUtc)),
                Url = $"/publications/{x.Slug}"
            })
            .ToList();

        // Data sources: the editor's list; otherwise the official sources registered for the
        // country plus every source cited by the snapshot cards and the GDP chart, looked up in
        // the Sources section (all countries and regional ones) for their description and link.
        var dataSources = profile.Sources;
        if (dataSources.Count == 0)
        {
            var registeredSources = await _dbContext.ContentSources
                .AsNoTracking()
                .Where(x => x.IsPublished)
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => new { x.Name, x.Summary, x.Url, x.CountryCode })
                .ToListAsync(cancellationToken);

            var citedNames = snapshotKpis.Select(x => x.Source)
                .Concat(gdpHistory.Select(x => x.Source))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            dataSources = registeredSources
                .Where(x => string.Equals(x.CountryCode, code, StringComparison.OrdinalIgnoreCase))
                .Select(x => new Domain.Content.CountryProfileSource { Institution = x.Name, Description = x.Summary, Url = x.Url })
                .Concat(citedNames.Select(name =>
                {
                    var registered = registeredSources.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
                    return new Domain.Content.CountryProfileSource { Institution = name, Description = registered?.Summary ?? string.Empty, Url = registered?.Url ?? string.Empty };
                }))
                .DistinctBy(x => x.Institution, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return new CountryProfilePageViewModel
        {
            Country = new CountryOverviewCardViewModel
            {
                Code = entity.Code,
                Name = entity.Name,
                Capital = entity.Capital,
                Summary = entity.Summary,
                Gdp = entity.Gdp,
                Population = entity.Population,
                BankAssets = entity.BankAssets,
                Slug = entity.Slug,
                HeroClassName = entity.HeroClassName,
                HeroImageUrl = heroGallery
                    .Select(item => item.ImageUrl)
                    .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url)) ?? string.Empty,
                IndicatorLabel = "Indicator",
                IndicatorValue = "No data"
            },
            Title = string.IsNullOrWhiteSpace(entity.OfficialName) ? entity.Name : entity.OfficialName,
            Region = entity.Region,
            FlagImageUrl = string.IsNullOrWhiteSpace(entity.FlagImageUrl) ? CountryFlags.IconUrl(entity.Code) : entity.FlagImageUrl,
            HeroDescription = string.IsNullOrWhiteSpace(entity.ProfileLead) ? entity.Summary : entity.ProfileLead,
            UpdatedLabel = entity.UpdatedAtUtc.UtcDateTime.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant(),
            HeroFacts = BuildHeroFacts(entity),
            DataSource = entity.DataSource,
            HeroBackgroundUrl = heroGallery
                .Select(item => item.ImageUrl)
                .FirstOrDefault(url => !string.IsNullOrWhiteSpace(url)) ?? string.Empty,
            HeroGallery = heroGallery.Where(item => !string.IsNullOrWhiteSpace(item.ImageUrl)).ToList(),
            AboutParagraphs = SplitParagraphs(string.IsNullOrWhiteSpace(entity.AboutText) ? entity.Summary : entity.AboutText),
            AboutFacts = new CountryHeroFactViewModel[]
                {
                    new() { Label = "Region", Value = entity.Region },
                    new() { Label = "Area", Value = entity.Area },
                    new() { Label = "Neighbouring countries", Value = entity.NeighbouringCountries },
                    new() { Label = "Official languages", Value = entity.OfficialLanguages }
                }
                .Where(item => !string.IsNullOrWhiteSpace(item.Value))
                .ToList(),
            Profile = profile,
            SnapshotKpis = snapshotKpis,
            GdpHistory = gdpHistory,
            DataSources = dataSources,
            LatestUpdates = latestUpdates,
            RelatedPublications = relatedPublications
        };
    }

    // Headline facts in the profile header; a fact without a value is left out.
    private static List<CountryHeroFactViewModel> BuildHeroFacts(Domain.Content.CountryContent entity)
    {
        // "Kyrgyz som (KGS)" is shown as "Kyrgyz som" with "KGS" on the detail line.
        var currency = entity.Currency.Trim();
        var currencyCode = string.Empty;
        var codeStart = currency.LastIndexOf('(');
        if (codeStart > 0 && currency.EndsWith(')'))
        {
            currencyCode = currency[(codeStart + 1)..^1].Trim();
            currency = currency[..codeStart].Trim();
        }

        return new CountryHeroFactViewModel[]
            {
                new() { Label = "Population", Value = entity.Population, Note = entity.PopulationPeriod },
                new() { Label = "GDP · Nominal", Value = entity.Gdp, Note = entity.GdpPeriod },
                new() { Label = "Currency", Value = currency, Note = currencyCode },
                new() { Label = "Capital", Value = entity.Capital, Note = string.IsNullOrWhiteSpace(entity.OfficialLanguages) ? string.Empty : $"Official languages: {entity.OfficialLanguages}" }
            }
            .Where(item => !string.IsNullOrWhiteSpace(item.Value))
            .ToList();
    }

    private static List<string> SplitParagraphs(string? text) =>
        (text ?? string.Empty)
            .Replace("\r\n", "\n")
            .Split("\n\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

    private static string FormatIndicatorValue(string value, string unit) =>
        string.IsNullOrWhiteSpace(unit) ? value
        : unit == "%" ? $"{value}%"
        : $"{value} {unit}";

    private static string JoinMeta(params string[] parts) =>
        string.Join(" · ", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
}
