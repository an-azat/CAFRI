using System.Text.Json;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Countries;
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

    public CountriesPageViewModel GetIndexPage()
    {
        var publishedIndicatorCount = _dbContext.CountryIndicators.AsNoTracking().Count(x => x.IsPublished);
        var indicatorCategories = _dbContext.CountryIndicators
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .Select(x => x.Category)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var countryRows = _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToList();

        var countries = countryRows
            .Select(x => new CountryOverviewCardViewModel
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
                Metrics =
                [
                    new() { Icon = "chart", Label = "GDP (2025)", Value = x.Gdp },
                    new() { Icon = "people", Label = "Population", Value = x.Population },
                    new() { Icon = "bank", Label = "Bank Assets", Value = x.BankAssets }
                ]
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

    public CountryProfilePageViewModel? GetDetails(string slug)
    {
        var entity = _dbContext.CountryContents
            .AsNoTracking()
            .FirstOrDefault(x => x.IsPublished && x.Slug == slug);

        if (entity is null)
        {
            return null;
        }

        var details = JsonSerializer.Deserialize<CountryProfilePageViewModel>(entity.DetailsJson, JsonOptions);
        if (details is null)
        {
            return null;
        }

        details.HeroGallery = ContentImageTextSerializer.Parse(entity.HeroGalleryJson);

        return details;
    }
}
