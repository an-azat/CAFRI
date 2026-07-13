using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Countries;
using CAFRI.ViewModels.Map;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public sealed class DbMapContentService : IMapContentService
{
    private static readonly IReadOnlyDictionary<string, MapShapeDefinition> ShapeDefinitions =
        new Dictionary<string, MapShapeDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["KZ"] = new(
                "M 670.3,182.3 663.3,182.0 658.8,190.8 646.0,193.3 642.9,200.0 645.4,209.8 642.6,213.2 631.1,216.8 629.9,214.2 618.4,214.4 605.0,210.8 593.9,236.3 599.0,238.1 597.6,242.5 588.7,242.2 584.4,239.1 556.9,245.8 566.5,248.5 564.5,249.6 564.2,257.7 571.1,272.5 564.6,274.3 567.4,276.6 562.5,277.9 562.8,286.8 557.8,283.5 550.3,282.8 546.1,277.9 512.8,274.9 495.2,275.7 492.5,277.7 470.6,270.8 461.4,274.3 459.0,280.4 460.4,283.8 435.2,277.3 426.0,278.6 422.3,281.4 421.5,286.2 416.4,289.5 412.5,289.0 392.4,299.5 392.4,302.0 385.4,306.5 386.0,311.1 383.7,311.5 376.6,308.0 378.7,304.7 375.7,302.1 357.4,302.8 354.0,290.1 346.4,289.9 347.8,274.9 343.2,276.6 339.0,270.2 329.9,263.8 322.4,266.4 303.9,265.2 285.9,267.5 270.9,253.7 233.2,235.9 194.0,244.4 194.0,300.3 186.0,300.9 177.7,291.0 165.8,284.8 148.4,288.0 140.2,293.8 139.7,288.6 143.8,281.8 142.1,278.7 144.4,281.1 144.8,279.3 132.2,277.2 128.3,271.9 122.6,272.4 123.1,267.7 116.0,256.7 107.4,254.5 106.6,250.9 126.9,251.8 117.8,246.4 122.5,244.0 124.7,238.4 152.7,239.2 144.7,235.9 150.6,227.4 148.7,221.5 151.9,218.6 149.7,215.6 140.5,214.0 135.3,216.8 121.6,212.1 100.4,221.0 92.6,220.2 93.6,223.1 91.4,222.0 91.4,224.2 79.7,219.1 88.1,217.5 75.4,203.1 63.8,201.3 63.0,203.6 59.5,201.7 59.1,194.7 49.7,192.5 57.9,182.1 54.2,178.5 56.4,170.5 62.6,167.3 62.2,163.9 66.7,161.5 76.4,170.4 84.7,169.4 86.1,167.3 81.4,158.7 93.6,155.8 94.6,151.5 108.1,148.6 112.0,143.5 115.2,144.1 114.8,141.7 124.0,143.6 123.0,146.0 128.9,146.3 131.6,143.0 138.2,141.4 141.9,146.2 154.9,145.8 171.2,155.4 169.7,159.0 171.5,160.4 174.2,159.2 171.6,154.3 173.7,152.8 189.2,160.3 201.6,152.1 204.8,153.7 211.9,151.7 216.6,155.3 220.5,154.6 221.2,151.4 229.4,150.9 233.8,152.8 234.8,156.4 246.6,158.8 247.9,161.2 252.3,159.8 255.5,155.4 266.0,158.5 276.0,156.5 280.4,149.3 277.5,146.7 260.8,143.1 262.0,141.0 254.9,138.7 270.9,132.9 265.4,127.8 269.9,123.2 287.0,122.9 286.3,120.9 272.8,118.3 273.9,115.2 278.8,115.1 268.5,113.2 273.4,110.7 270.3,108.5 291.8,107.2 293.5,109.6 294.4,106.8 303.1,104.9 324.7,101.7 333.9,102.9 334.1,99.8 337.9,98.1 379.2,93.1 381.4,91.3 379.4,89.8 385.9,89.4 387.3,86.9 391.4,88.2 391.0,85.8 410.3,90.4 419.6,87.9 422.4,95.8 426.3,97.6 425.2,102.8 421.8,103.0 424.7,106.1 432.1,106.0 433.9,103.8 439.8,105.7 438.1,101.9 444.8,105.5 443.4,109.0 448.1,108.2 445.1,106.8 447.0,105.6 458.2,108.5 463.9,106.7 463.3,109.1 463.1,109.5 456.2,112.7 458.9,116.3 466.4,113.1 473.8,115.8 474.5,112.6 479.6,110.2 511.9,100.6 508.4,105.6 504.0,105.3 505.8,107.7 526.6,118.7 559.4,156.7 565.7,153.8 565.5,150.3 569.4,148.6 576.6,150.4 575.0,153.9 580.3,153.8 581.2,157.3 596.7,157.5 600.5,154.7 611.2,153.3 619.1,156.5 623.6,164.4 634.8,167.4 638.4,174.5 652.9,176.5 661.6,171.7 659.3,174.9 670.1,181.9 670.3,182.3 Z",
                333.4m, 189.0m, 430m, 140m, 440m, 143m),
            ["UZ"] = new(
                "M 421.7,316.7 416.5,317.3 412.7,314.8 412.7,313.1 418.7,309.1 413.9,304.8 402.4,310.8 397.3,308.5 394.9,311.7 396.2,317.4 385.5,317.9 391.7,319.0 388.5,319.1 390.1,322.2 388.2,320.6 384.6,327.2 372.4,326.1 368.0,328.3 366.8,332.2 371.6,333.5 372.0,335.7 378.2,335.9 379.3,337.9 377.5,342.6 382.1,347.5 374.1,357.9 373.4,363.1 369.1,361.7 365.4,363.1 361.6,360.1 354.2,360.4 355.8,350.8 344.4,346.6 340.3,347.1 321.0,336.0 318.2,336.3 311.5,332.6 292.4,320.2 290.9,313.8 287.6,311.6 283.7,303.3 276.9,300.6 274.6,302.8 262.4,301.9 256.6,299.2 257.6,296.3 255.9,293.7 259.0,293.3 253.7,290.5 255.6,286.9 243.9,284.7 242.2,282.0 238.3,281.6 233.7,277.9 232.9,280.0 226.6,280.5 232.2,285.4 225.0,282.4 222.5,287.0 215.4,287.5 209.2,291.6 209.8,298.1 212.0,299.6 210.0,301.2 194.0,300.3 194.0,244.4 233.2,235.9 270.9,253.7 285.9,267.5 303.9,265.2 322.4,266.4 329.9,263.8 339.0,270.2 343.2,276.6 347.8,274.9 346.4,289.9 354.0,290.1 357.4,302.8 375.7,302.1 378.7,304.7 376.6,308.0 383.7,311.5 386.0,311.1 385.4,306.5 392.4,302.0 392.4,299.5 405.8,294.3 412.5,289.0 415.9,289.7 418.2,287.0 423.5,285.7 425.9,287.5 419.9,289.5 409.6,297.2 414.0,299.0 417.2,298.0 418.8,301.5 425.0,303.5 428.6,303.2 428.7,299.9 431.2,300.2 432.5,296.8 435.5,302.2 439.9,302.3 440.0,305.2 442.5,304.0 447.5,307.2 454.9,308.0 447.0,312.6 442.8,311.2 443.5,314.4 440.0,313.4 436.8,315.6 437.8,316.7 433.0,318.2 426.1,315.4 422.2,316.3 421.7,316.7 Z",
                352.5m, 306.4m, 385m, 286m, 395m, 289m),
            ["KG"] = new(
                "M 562.2,287.1 562.6,289.4 556.7,289.9 534.1,299.4 529.8,304.6 511.4,305.0 503.3,315.1 502.0,313.6 493.4,315.9 491.6,310.7 486.0,313.6 480.6,312.6 481.0,315.4 467.4,319.8 465.2,323.3 466.8,326.5 465.2,328.4 445.4,329.8 441.0,332.7 438.2,329.9 433.7,331.4 433.5,328.7 430.1,328.6 429.3,326.2 418.8,329.7 414.3,326.4 396.5,327.7 395.6,322.9 396.7,320.5 399.5,321.5 399.8,318.4 407.2,317.2 415.1,319.8 414.3,321.9 422.2,316.3 426.1,315.4 433.0,318.2 437.8,316.7 436.8,315.6 440.0,313.4 443.5,314.4 442.8,311.2 447.0,312.6 454.9,308.0 447.5,307.2 442.5,304.0 440.0,305.2 439.9,302.3 435.5,302.2 432.5,296.8 431.2,300.2 428.7,299.9 428.6,303.2 425.0,303.5 418.8,301.5 417.2,298.0 414.0,299.0 409.6,297.2 419.9,289.5 425.9,287.5 420.0,285.3 423.3,282.8 422.3,281.4 428.4,277.9 441.9,278.4 460.4,283.8 459.0,280.4 461.4,274.3 470.6,270.8 485.8,277.0 492.5,277.7 495.2,275.7 512.8,274.9 546.1,277.9 550.3,282.8 557.8,283.5 561.6,286.8 562.2,287.1 Z",
                455.8m, 305.1m, 470m, 304m, 480m, 307m),
            ["TJ"] = new(
                "M 421.7,316.7 414.3,321.9 415.1,319.8 407.2,317.2 399.8,318.4 399.5,321.5 396.7,320.5 395.6,322.9 396.5,327.7 414.3,326.4 418.8,329.7 429.3,326.2 430.1,328.6 433.5,328.7 433.7,331.4 438.2,329.9 441.0,332.7 445.4,329.8 461.9,328.6 461.9,332.4 465.2,335.9 463.0,337.5 465.1,341.8 468.3,342.7 469.8,340.6 473.0,340.6 480.5,343.7 479.9,349.5 482.5,354.3 481.4,356.8 485.6,359.9 481.4,362.4 477.9,360.0 472.7,359.5 463.5,362.6 461.8,362.0 464.1,359.4 457.2,358.9 449.5,362.6 447.5,365.5 432.2,370.7 428.8,364.9 431.4,352.3 426.2,351.9 427.7,346.9 421.5,343.6 417.2,344.6 409.7,351.6 411.4,355.2 409.5,357.7 399.9,357.0 396.8,364.1 390.2,360.8 377.0,367.1 373.3,364.3 373.7,358.9 382.1,347.5 377.5,342.6 379.3,337.9 378.2,335.9 372.0,335.7 371.6,333.5 366.8,332.2 368.0,328.3 372.4,326.1 384.6,327.2 388.2,320.6 390.1,322.2 388.5,319.1 391.7,319.0 385.5,317.9 396.2,317.4 394.9,311.7 397.3,308.5 402.8,310.7 411.4,307.0 412.2,306.3 412.6,304.7 418.7,309.1 412.7,313.1 412.7,314.8 416.5,317.3 421.4,316.7 421.7,316.7 Z",
                421.4m, 336.9m, 421m, 344m, 431m, 347m),
            ["TM"] = new(
                "M 274.4,386.9 272.3,381.6 273.7,379.3 272.8,371.5 260.3,371.5 255.4,365.5 247.0,362.8 244.7,357.9 236.9,355.4 230.2,356.3 227.1,354.0 214.9,351.2 212.4,346.5 210.1,347.9 201.0,346.9 197.6,349.7 186.0,349.5 177.0,354.4 173.9,359.3 162.6,360.7 161.3,342.8 163.7,336.8 159.4,334.4 159.8,331.9 159.1,334.3 157.4,332.6 157.3,330.2 151.0,330.3 151.2,332.8 150.1,329.5 153.0,325.4 152.1,327.6 159.9,327.5 155.1,325.7 154.6,323.9 157.5,320.8 147.4,320.0 148.9,323.9 144.8,319.5 144.8,313.0 147.5,303.9 150.6,308.7 154.4,307.7 156.1,309.2 158.3,306.9 159.7,310.8 170.1,310.1 167.8,306.8 173.7,307.6 175.5,305.7 174.3,304.1 175.6,304.6 165.1,297.7 162.5,289.0 159.1,287.8 151.2,288.4 147.8,290.6 145.3,294.9 147.5,294.9 145.9,299.4 146.9,304.4 140.2,293.8 151.7,287.0 165.8,284.8 177.7,291.0 186.4,301.1 209.7,301.3 212.0,299.6 209.8,298.1 209.2,291.6 215.4,287.5 222.5,287.0 225.0,282.4 232.2,285.4 226.6,280.5 232.9,280.0 233.7,277.9 238.3,281.6 242.2,282.0 243.9,284.7 255.6,286.9 253.7,290.5 259.0,293.3 255.9,293.7 257.6,296.3 256.6,299.2 262.4,301.9 274.6,302.8 276.9,300.6 283.7,303.3 287.6,311.6 290.9,313.8 292.4,320.2 311.5,332.6 318.2,336.3 321.0,336.0 340.3,347.1 344.4,346.6 355.8,350.8 354.2,360.4 350.8,360.9 342.7,357.1 339.3,362.0 328.2,363.9 323.2,377.0 316.8,379.5 316.6,381.2 302.3,383.3 304.0,385.7 301.9,386.8 301.9,389.7 296.1,392.6 292.1,392.1 289.9,394.2 285.6,389.7 279.5,389.8 274.6,386.9 274.4,386.9 Z",
                222.4m, 328.0m, 200m, 334m, 210m, 337m)
        };

    private readonly AppDbContext _dbContext;

    public DbMapContentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public MapPageViewModel GetIndexPage()
    {
        var countries = _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToList();

        var indicators = _dbContext.CountryIndicators
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.DisplayOrder)
            .ToList();

        var intelligenceCount = _dbContext.IntelligenceContentItems.AsNoTracking().Count(x => x.IsPublished);
        var publishedIndicatorsCount = indicators.Count;
        var categoryOptions = indicators
            .Select(x => x.Category)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();

        var selectedIndicatorName = indicators
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault() ?? "Real GDP Growth";

        var indicatorOptions = indicators
            .Select(x => x.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order()
            .ToList();

        var countryNodes = countries
            .Select(country =>
            {
                var countryIndicators = indicators
                    .Where(x => string.Equals(x.CountryCode, country.Code, StringComparison.OrdinalIgnoreCase))
                    .Select(x => new MapCountryIndicatorValueViewModel
                    {
                        Name = x.Name,
                        Label = $"{x.Name} {x.YearLabel}".Trim(),
                        Value = string.IsNullOrWhiteSpace(x.Unit) ? x.LatestValue : $"{x.LatestValue} {x.Unit}",
                        Category = x.Category,
                        Unit = x.Unit,
                        YearLabel = x.YearLabel,
                        SourceName = x.SourceName
                    })
                    .ToList();

                var indicator = indicators.FirstOrDefault(x =>
                    string.Equals(x.CountryCode, country.Code, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.Name, selectedIndicatorName, StringComparison.OrdinalIgnoreCase));
                var shape = ShapeDefinitions.TryGetValue(country.Code, out var shapeDefinition)
                    ? shapeDefinition
                    : ShapeDefinitions["KZ"];

                return new MapCountryNodeViewModel
                {
                    Code = country.Code,
                    Name = country.Name,
                    Slug = country.Slug,
                    Capital = country.Capital,
                    Gdp = country.Gdp,
                    Population = country.Population,
                    BankAssets = country.BankAssets,
                    IndicatorLabel = indicator is null ? "Indicator" : $"{indicator.Name} {indicator.YearLabel}",
                    IndicatorValue = indicator?.LatestValue ?? "No data",
                    Summary = country.Summary,
                    AccentClass = country.Code.ToUpperInvariant() switch
                    {
                        "KZ" => "map-country--kazakhstan",
                        "UZ" => "map-country--uzbekistan",
                        "KG" => "map-country--kyrgyzstan",
                        "TJ" => "map-country--tajikistan",
                        "TM" => "map-country--turkmenistan",
                        _ => "map-country--kazakhstan"
                    },
                    PathData = shape.PathData,
                    LabelX = shape.LabelX,
                    LabelY = shape.LabelY,
                    CapitalX = shape.CapitalX,
                    CapitalY = shape.CapitalY,
                    CapitalLabelX = shape.CapitalLabelX,
                    CapitalLabelY = shape.CapitalLabelY,
                    Indicators = countryIndicators
                };
            })
            .ToList();

        return new MapPageViewModel
        {
            Description = "Explore financial regulation, banking sector, macroeconomic indicators, and institutional developments across Central Asia.",
            SummaryCards =
            [
                new CountryStatsCardViewModel { Icon = "globe", Value = countries.Count.ToString(), Title = "Countries", Description = "Central Asia region" },
                new CountryStatsCardViewModel { Icon = "institution", Value = publishedIndicatorsCount.ToString(), Title = "Data Indicators", Description = "Financial, regulatory & macroeconomic" },
                new CountryStatsCardViewModel { Icon = "document", Value = intelligenceCount.ToString(), Title = "Intelligence Items", Description = "Latest updates & publications" },
                new CountryStatsCardViewModel { Icon = "calendar", Value = "Updated", Title = "Live CMS", Description = "Database-backed content" }
            ],
            DataCategories = categoryOptions
                .Select((label, index) => new MapLayerOptionViewModel
                {
                    Icon = index % 2 == 0 ? "chart" : "institution",
                    Label = label,
                    IsSelected = true
                })
                .ToList(),
            MapStyles = ["Dark", "Light", "Satellite"],
            SelectedMapStyle = "Dark",
            Overlays =
            [
                new() { Icon = "trade", Label = "Economic Corridors" },
                new() { Icon = "gateway", Label = "Border Crossings" },
                new() { Icon = "bank", Label = "Financial Centers" }
            ],
            IndicatorOptions = indicatorOptions.Count == 0 ? ["Real GDP Growth"] : indicatorOptions,
            SelectedIndicator = selectedIndicatorName,
            LegendItems =
            [
                new() { Label = "6% and above", ColorClass = "map-legend__swatch--teal" },
                new() { Label = "4% - 6%", ColorClass = "map-legend__swatch--blue" },
                new() { Label = "2% - 4%", ColorClass = "map-legend__swatch--amber" },
                new() { Label = "0% - 2%", ColorClass = "map-legend__swatch--orange" },
                new() { Label = "Below 0%", ColorClass = "map-legend__swatch--red" },
                new() { Label = "No data", ColorClass = "map-legend__swatch--gray" }
            ],
            Countries = countryNodes,
            CountryLookup = countryNodes.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase)
        };
    }

    private sealed record MapShapeDefinition(
        string PathData,
        decimal LabelX,
        decimal LabelY,
        decimal CapitalX,
        decimal CapitalY,
        decimal CapitalLabelX,
        decimal CapitalLabelY);
}
