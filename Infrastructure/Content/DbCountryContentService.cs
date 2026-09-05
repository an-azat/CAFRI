using System.Text.Json;
using CAFRI.Application.Abstractions.Services;
using CAFRI.ViewModels.Shared;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Countries;
using CAFRI.ViewModels.Map;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public sealed class DbCountryContentService : ICountryContentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;

    public DbCountryContentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CountriesPageViewModel> GetIndexPageAsync(CancellationToken cancellationToken = default)
    {
        var publishedIndicatorCount = await _dbContext.CountryIndicators.AsNoTracking().CountAsync(x => x.IsPublished, cancellationToken);
        var indicatorCategories = await _dbContext.CountryIndicators
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cancellationToken);

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
            Description = "Country intelligence profiles covering financial regulation, banking sector, macroeconomic indicators, trade, sanctions exposure, and institutional developments across Central Asia and adjacent economies.",
            StatsCards =
            [
                new() { Icon = "institution", Value = countries.Count.ToString(), Title = "Countries", Description = "Comprehensive coverage across the region" },
                new() { Icon = "chart", Value = publishedIndicatorCount.ToString(), Title = "Indicators Tracked", Description = "Financial, regulatory, and macroeconomic data" }
            ],
            Countries = countries,
            GdpGrowthYears = gdpGrowthYears,
            SelectedGdpGrowthYear = selectedGdpGrowthYear,
            CompareFeatures = indicatorCategories.Select(category => new CountryCompareFeatureViewModel
            {
                Icon = category.Contains("bank", StringComparison.OrdinalIgnoreCase) ? "bank" :
                    category.Contains("sanction", StringComparison.OrdinalIgnoreCase) ? "check" :
                    category.Contains("trade", StringComparison.OrdinalIgnoreCase) ? "trade" :
                    category.Contains("regulation", StringComparison.OrdinalIgnoreCase) ? "shield" :
                    "chart",
                Title = category,
                Description = $"{category} indicators and supporting country intelligence"
            }).DefaultIfEmpty(new CountryCompareFeatureViewModel
            {
                Icon = "chart",
                Title = "Macroeconomic Overview",
                Description = "GDP, inflation, trade, FX reserves"
            }).ToList()
        };
    }

    public async Task<CountryProfilePageViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.CountryContents
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IsPublished && x.Slug == slug, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        CountryProfilePageViewModel? details;
        try
        {
            details = JsonSerializer.Deserialize<CountryProfilePageViewModel>(entity.DetailsJson, JsonOptions);
        }
        catch (JsonException)
        {
            return BuildFallbackDetails(entity);
        }

        if (details is null)
        {
            return BuildFallbackDetails(entity);
        }

        details.HeroGallery = ContentImageTextSerializer.Parse(entity.HeroGalleryJson);

        return details;
    }

    // Mirrors the BuildFallbackDetails pattern used by DbIntelligenceContentService and
    // DbPublicationContentService: if DetailsJson is missing or fails to deserialize, degrade
    // to a minimal-but-valid profile built from the base CountryContent row instead of 404ing
    // the whole page.
    private static CountryProfilePageViewModel BuildFallbackDetails(Domain.Content.CountryContent entity)
    {
        var heroGallery = ContentImageTextSerializer.Parse(entity.HeroGalleryJson);

        var country = new CountryOverviewCardViewModel
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
            IndicatorValue = "No data",
            Indicators = [],
            Metrics =
            [
                new() { Icon = "chart", Label = "GDP (2025)", Value = entity.Gdp },
                new() { Icon = "people", Label = "Population", Value = entity.Population },
                new() { Icon = "bank", Label = "Bank Assets", Value = entity.BankAssets }
            ]
        };

        var emptyChart = new CountryChartSeriesViewModel { Title = string.Empty, ValueSuffix = string.Empty };

        return new CountryProfilePageViewModel
        {
            Country = country,
            HeroDescription = string.IsNullOrWhiteSpace(entity.Summary)
                ? $"A detailed profile for {entity.Name} is being prepared."
                : entity.Summary,
            Currency = "Data unavailable",
            RiskRating = "N/A",
            RiskOutlook = "Data unavailable",
            DoingBusinessRank = "N/A",
            DoingBusinessSource = "Data unavailable",
            OverviewText = string.IsNullOrWhiteSpace(entity.Summary)
                ? $"A detailed profile for {entity.Name} is being prepared. Please check back soon for full financial, regulatory, and macroeconomic coverage."
                : entity.Summary,
            HeroGallery = heroGallery,
            HeroIndicators =
            [
                new() { Icon = "capital", Label = "Capital", Value = entity.Capital },
                new() { Icon = "population", Label = "Population (2025)", Value = entity.Population },
                new() { Icon = "currency", Label = "GDP (2025)", Value = entity.Gdp }
            ],
            KeyIndicators = [],
            Tabs = [new() { Label = "Overview", IsActive = true }],
            Highlights = [],
            MacroSummaryCards = [],
            MacroIndicatorRows = [],
            GdpGrowthChart = emptyChart,
            InflationChart = emptyChart,
            FinancialSectorSnapshot = [],
            BankingAssetsChart = emptyChart,
            TopBanks = [],
            LatestUpdates = [],
            RelatedPublications = [],
            KeyInstitutions = [],
            ExploreLinks = []
        };
    }
}
