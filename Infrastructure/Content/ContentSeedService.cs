using System.Text.Json;
using CAFRI.Domain.Content;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Countries;
using CAFRI.ViewModels.Intelligence;
using CAFRI.ViewModels.Publications;
using CAFRI.ViewModels.Home;

namespace CAFRI.Infrastructure.Content;

public sealed class ContentSeedService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;
    private readonly JsonContentFileLoader _loader;

    public ContentSeedService(AppDbContext dbContext, JsonContentFileLoader loader)
    {
        _dbContext = dbContext;
        _loader = loader;
    }

    public async Task SeedAsync()
    {
        if (!_dbContext.IntelligenceContentItems.Any())
        {
            var intelligence = _loader.Load<IntelligenceContentDocument>("App_Data/content/intelligence.json");
            var detailMap = intelligence.Details.ToDictionary(x => x.Slug, StringComparer.OrdinalIgnoreCase);
            var now = DateTimeOffset.UtcNow;

            foreach (var (item, index) in intelligence.Items.Select((item, index) => (item, index)))
            {
                _dbContext.IntelligenceContentItems.Add(new IntelligenceContentItem
                {
                    Id = Guid.NewGuid(),
                    Slug = item.Slug,
                    Category = item.Category,
                    CountryCode = item.CountryCode,
                    CountryName = item.CountryName,
                    PublishedLabel = item.PublishedLabel,
                    Title = item.Title,
                    Description = item.Description,
                    Source = item.Source,
                    DetailsJson = detailMap.TryGetValue(item.Slug, out var detail) ? JsonSerializer.Serialize(detail) : null,
                    DisplayOrder = index,
                    IsPublished = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        if (!_dbContext.PublicationContentItems.Any())
        {
            var publications = _loader.Load<PublicationsContentDocument>("App_Data/content/publications.json");
            var detailMap = publications.Details.ToDictionary(x => x.Slug, StringComparer.OrdinalIgnoreCase);
            var now = DateTimeOffset.UtcNow;

            foreach (var (item, index) in publications.Publications.Select((item, index) => (item, index)))
            {
                detailMap.TryGetValue(item.Slug, out var detail);

                _dbContext.PublicationContentItems.Add(new PublicationContentItem
                {
                    Id = Guid.NewGuid(),
                    Slug = item.Slug,
                    Type = item.Type,
                    CountryCode = item.CountryCode,
                    CountryLabel = item.CountryLabel,
                    Title = item.Title,
                    Description = item.Description,
                    Meta = item.Meta,
                    Topic = PublicationTopicMapper.Normalize(detail?.Topic, item.Type, item.Title, item.Description),
                    PublishedDate = item.Meta.Split('|')[0].Trim(),
                    ReadingTime = item.Meta.Contains('|', StringComparison.Ordinal) ? item.Meta.Split('|')[1].Trim() : "12 min read",
                    AuthorLabel = detail?.AuthorLabel ?? "CAFRI Research Team",
                    DocumentLabel = detail?.DocumentLabel ?? "PDF available | 1.2 MB",
                    HeroVisualClassName = detail?.HeroVisualClassName ?? "publication-hero-media--trade-corridors",
                    PdfDownloadUrl = detail?.PdfDownloadUrl ?? "#",
                    DetailsJson = detail is null ? null : JsonSerializer.Serialize(detail),
                    DisplayOrder = index,
                    IsPublished = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }
        else
        {
            var existingPublications = _dbContext.PublicationContentItems.ToList();
            foreach (var item in existingPublications)
            {
                var normalizedTopic = PublicationTopicMapper.Normalize(item.Topic, item.Type, item.Title, item.Description);
                if (!string.Equals(item.Topic, normalizedTopic, StringComparison.Ordinal))
                {
                    item.Topic = normalizedTopic;
                    item.UpdatedAtUtc = DateTimeOffset.UtcNow;
                }
            }
        }

        if (!_dbContext.CountryContents.Any())
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var (country, index) in BuildCountries().Select((item, index) => (item, index)))
            {
                _dbContext.CountryContents.Add(new CountryContent
                {
                    Id = Guid.NewGuid(),
                    Code = country.Code,
                    Slug = country.Slug,
                    Name = country.Name,
                    Capital = country.Capital,
                    Summary = country.Summary,
                    Gdp = country.Gdp,
                    Population = country.Population,
                    BankAssets = country.BankAssets,
                    HeroClassName = country.HeroClassName,
                    DetailsJson = JsonSerializer.Serialize(BuildCountryProfile(country)),
                    DisplayOrder = index,
                    IsPublished = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        if (!_dbContext.ContentCategories.Any())
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var (item, index) in BuildCategories().Select((item, index) => (item, index)))
            {
                _dbContext.ContentCategories.Add(new ContentCategory
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Slug = item.Slug,
                    Description = item.Description,
                    Scope = item.Scope,
                    DisplayOrder = index,
                    IsPublished = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        if (!_dbContext.ContentSources.Any())
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var (item, index) in BuildSources().Select((item, index) => (item, index)))
            {
                _dbContext.ContentSources.Add(new ContentSource
                {
                    Id = Guid.NewGuid(),
                    Name = item.Name,
                    Slug = item.Slug,
                    SourceType = item.SourceType,
                    CountryCode = item.CountryCode,
                    Url = item.Url,
                    Summary = item.Summary,
                    DisplayOrder = index,
                    IsPublished = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }

        if (!_dbContext.CountryIndicators.Any())
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var (item, index) in BuildIndicators().Select((item, index) => (item, index)))
            {
                _dbContext.CountryIndicators.Add(new CountryIndicator
                {
                    Id = Guid.NewGuid(),
                    CountryCode = item.CountryCode,
                    Name = item.Name,
                    Slug = item.Slug,
                    Category = item.Category,
                    Unit = item.Unit,
                    LatestValue = item.LatestValue,
                    YearLabel = item.YearLabel,
                    SourceName = item.SourceName,
                    Notes = item.Notes,
                    DisplayOrder = index,
                    IsPublished = true,
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
            }
        }
        else
        {
            // Older seeded databases only carry a single latest-year "Real GDP Growth" row per
            // country. Replace those with the full multi-year history from BuildIndicators() so the
            // countries map's year selector has data to switch between, without duplicating rows.
            var existingGrowthRows = _dbContext.CountryIndicators
                .Where(x => x.Name == "Real GDP Growth")
                .ToList();
            var hasFullGrowthHistory = existingGrowthRows
                .Select(x => x.YearLabel)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count() >= 3;

            if (!hasFullGrowthHistory)
            {
                var baseOrder = _dbContext.CountryIndicators.Any()
                    ? _dbContext.CountryIndicators.Max(x => x.DisplayOrder) + 1
                    : 0;
                _dbContext.CountryIndicators.RemoveRange(existingGrowthRows);

                var now = DateTimeOffset.UtcNow;
                foreach (var (item, offset) in BuildIndicators().Where(x => x.Name == "Real GDP Growth").Select((item, offset) => (item, offset)))
                {
                    _dbContext.CountryIndicators.Add(new CountryIndicator
                    {
                        Id = Guid.NewGuid(),
                        CountryCode = item.CountryCode,
                        Name = item.Name,
                        Slug = item.Slug,
                        Category = item.Category,
                        Unit = item.Unit,
                        LatestValue = item.LatestValue,
                        YearLabel = item.YearLabel,
                        SourceName = item.SourceName,
                        Notes = item.Notes,
                        DisplayOrder = baseOrder + offset,
                        IsPublished = true,
                        CreatedAtUtc = now,
                        UpdatedAtUtc = now
                    });
                }
            }
        }

        if (!_dbContext.HomePageContents.Any())
        {
            var now = DateTimeOffset.UtcNow;
            var featuredPublicationId = _dbContext.PublicationContentItems
                .Where(x => x.Slug == "tajikistan-banking-sector-outlook")
                .Select(x => (Guid?)x.Id)
                .FirstOrDefault();

            _dbContext.HomePageContents.Add(new HomePageContent
            {
                Id = Guid.NewGuid(),
                HeroTitle = "Central Asia Financial & Regulatory Intelligence Initiative",
                HeroLead = "Your gateway to country profiles, investment climate, regulatory developments, publications and regional intelligence across Central Asia.",
                HeroPrimaryCtaLabel = "Explore Countries ->",
                HeroPrimaryCtaUrl = "/countries",
                HeroSecondaryCtaLabel = "View Intelligence",
                HeroSecondaryCtaUrl = "/intelligence",
                IntroTitle = "One regional platform for Central Asia",
                IntroDescription = "CAFRI brings together structured country information, investment climate context, regulatory intelligence, publications and an interactive regional map in one place. The platform is built for investors, researchers, policy analysts and institutions.",
                PlatformSectionLabel = "Platform sections",
                PlatformSectionTitle = "What you can explore",
                PlatformSectionDescription = "Main entry points of the platform: countries, investment environment, regulatory monitoring, publications and the regional map.",
                DirectionsText = HomePageContentTextSerializer.JoinDirections(
                [
                    new() { Number = "01", Title = "Explore Countries", Description = "Access country-level profiles covering institutions, market environment, regulation and development priorities.", Url = "/countries", LinkLabel = "View country profiles ->" },
                    new() { Number = "02", Title = "Investment Climate", Description = "Understand investment conditions, sector priorities, infrastructure and regional trade corridors.", Url = "/about", LinkLabel = "Explore investment climate ->" },
                    new() { Number = "03", Title = "Regulatory Intelligence", Description = "Track regulatory developments, policy changes and institutional updates across Central Asia.", Url = "/intelligence", LinkLabel = "View updates ->" },
                    new() { Number = "04", Title = "Publications", Description = "Read research reports, policy briefs, country reviews and analytical materials.", Url = "/publications", LinkLabel = "View publications ->" },
                    new() { Number = "05", Title = "Regional Coverage", Description = "Navigate Central Asia through structured country profiles and explore regulation, investment climate and related publications.", Url = "/countries", LinkLabel = "Open country coverage ->", IsWide = true }
                ]),
                CountriesSectionLabel = "Explore countries",
                CountriesSectionTitle = "Country profiles across Central Asia",
                CountriesSectionDescription = "Each country page includes overview, institutions, regulation, investment environment, infrastructure and related publications.",
                CountriesVisualTitle = "Regional Country Coverage",
                CountriesVisualDescription = "A structured entry point to Central Asia. Open a country profile to access its regulatory environment, investment climate and publications.",
                FeaturesSectionLabel = "Core themes",
                FeaturesSectionTitle = "Investment climate, regulation and research",
                FeaturesSectionDescription = "Three core content pillars help users understand the region through a clear, structured and institutional-grade interface.",
                FeatureCardsText = HomePageContentTextSerializer.JoinFeatureCards(
                [
                    new() { Title = "Investment Climate", Description = "Market conditions, sectors, infrastructure, business environment and regional corridors.", Url = "/about", CssClassName = "home-v2-feature-card--investment" },
                    new() { Title = "Regulatory Intelligence", Description = "Policy changes, institutional updates and regulation-focused monitoring.", Url = "/intelligence", CssClassName = "home-v2-feature-card--regulatory" },
                    new() { Title = "Publications", Description = "Reports, research notes, country reviews and analytical materials.", Url = "/publications", CssClassName = "home-v2-feature-card--publications" }
                ]),
                LatestSectionLabel = "Latest materials",
                LatestSectionTitle = "Latest intelligence and publications",
                LatestSectionDescription = "The latest updates added through CAFRI content workflows: intelligence items, reports, briefs and regional analytical materials.",
                FeaturedPublicationEyebrow = "Featured Publication",
                LatestFeaturedLinkLabel = "Read more ->",
                LatestIntelligenceLinkLabel = "Read more ->",
                FeaturedPublicationId = featuredPublicationId,
                AboutTitle = "About CAFRI",
                AboutDescription = "CAFRI is designed as a regional intelligence initiative focused on Central Asia. The platform helps users explore country profiles, investment climate, regulatory environment, publications and regional context through a clean, structured interface.",
                CoverageAreasText = HomePageContentTextSerializer.JoinCoverageAreas(
                [
                    new() { Icon = "institution", Title = "Financial Regulation", Description = "Monitoring regulatory developments from central banks, financial supervisors, and government institutions." },
                    new() { Icon = "institution", Title = "Banking Intelligence", Description = "Analysis of monetary policy, banking sector performance, licensing, and supervisory actions." },
                    new() { Icon = "shield", Title = "Sanctions & Compliance", Description = "Tracking domestic and international sanctions, AML/CFT frameworks, and compliance requirements." },
                    new() { Icon = "truck", Title = "Trade & Logistics", Description = "Monitoring trade corridors, customs, infrastructure, and cross-border logistics developments." },
                    new() { Icon = "chart", Title = "Macroeconomics", Description = "Key economic indicators, forecasts, fiscal policy, and structural economic trends." },
                    new() { Icon = "globe", Title = "Geopolitical Risk", Description = "Analysis of political risk, regional dynamics, and their impact on financial and trade stability." }
                ]),
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });
        }
        else
        {
            var homePage = _dbContext.HomePageContents.OrderByDescending(x => x.UpdatedAtUtc).First();
            var hasChanges = false;
            var featuredPublicationId = _dbContext.PublicationContentItems
                .Where(x => x.Slug == "tajikistan-banking-sector-outlook")
                .Select(x => (Guid?)x.Id)
                .FirstOrDefault();

            hasChanges |= FillIfEmpty(() => homePage.HeroTitle, value => homePage.HeroTitle = value, "Central Asia Financial & Regulatory Intelligence Initiative");
            hasChanges |= FillIfEmpty(() => homePage.HeroLead, value => homePage.HeroLead = value, "Your gateway to country profiles, investment climate, regulatory developments, publications and regional intelligence across Central Asia.");
            hasChanges |= FillIfEmpty(() => homePage.HeroPrimaryCtaLabel, value => homePage.HeroPrimaryCtaLabel = value, "Explore Countries ->");
            hasChanges |= FillIfEmpty(() => homePage.HeroPrimaryCtaUrl, value => homePage.HeroPrimaryCtaUrl = value, "/countries");
            hasChanges |= FillIfEmpty(() => homePage.HeroSecondaryCtaLabel, value => homePage.HeroSecondaryCtaLabel = value, "View Intelligence");
            hasChanges |= FillIfEmpty(() => homePage.HeroSecondaryCtaUrl, value => homePage.HeroSecondaryCtaUrl = value, "/intelligence");
            hasChanges |= FillIfEmpty(() => homePage.IntroTitle, value => homePage.IntroTitle = value, "One regional platform for Central Asia");
            hasChanges |= FillIfEmpty(() => homePage.IntroDescription, value => homePage.IntroDescription = value, "CAFRI brings together structured country information, investment climate context, regulatory intelligence, publications and an interactive regional map in one place. The platform is built for investors, researchers, policy analysts and institutions.");
            hasChanges |= FillIfEmpty(() => homePage.PlatformSectionLabel, value => homePage.PlatformSectionLabel = value, "Platform sections");
            hasChanges |= FillIfEmpty(() => homePage.PlatformSectionTitle, value => homePage.PlatformSectionTitle = value, "What you can explore");
            hasChanges |= FillIfEmpty(() => homePage.PlatformSectionDescription, value => homePage.PlatformSectionDescription = value, "Main entry points of the platform: countries, investment environment, regulatory monitoring, publications and the regional map.");
            hasChanges |= FillIfEmpty(() => homePage.CountriesSectionLabel, value => homePage.CountriesSectionLabel = value, "Explore countries");
            hasChanges |= FillIfEmpty(() => homePage.CountriesSectionTitle, value => homePage.CountriesSectionTitle = value, "Country profiles across Central Asia");
            hasChanges |= FillIfEmpty(() => homePage.CountriesSectionDescription, value => homePage.CountriesSectionDescription = value, "Each country page includes overview, institutions, regulation, investment environment, infrastructure and related publications.");
            hasChanges |= FillIfEmpty(() => homePage.CountriesVisualTitle, value => homePage.CountriesVisualTitle = value, "Regional Country Coverage");
            hasChanges |= FillIfEmpty(() => homePage.CountriesVisualDescription, value => homePage.CountriesVisualDescription = value, "A structured entry point to Central Asia. Open a country profile to access its regulatory environment, investment climate and publications.");
            hasChanges |= FillIfEmpty(() => homePage.FeaturesSectionLabel, value => homePage.FeaturesSectionLabel = value, "Core themes");
            hasChanges |= FillIfEmpty(() => homePage.FeaturesSectionTitle, value => homePage.FeaturesSectionTitle = value, "Investment climate, regulation and research");
            hasChanges |= FillIfEmpty(() => homePage.FeaturesSectionDescription, value => homePage.FeaturesSectionDescription = value, "Three core content pillars help users understand the region through a clear, structured and institutional-grade interface.");
            hasChanges |= FillIfEmpty(() => homePage.LatestSectionLabel, value => homePage.LatestSectionLabel = value, "Latest materials");
            hasChanges |= FillIfEmpty(() => homePage.LatestSectionTitle, value => homePage.LatestSectionTitle = value, "Latest intelligence and publications");
            hasChanges |= FillIfEmpty(() => homePage.LatestSectionDescription, value => homePage.LatestSectionDescription = value, "The latest updates added through CAFRI content workflows: intelligence items, reports, briefs and regional analytical materials.");
            hasChanges |= FillIfEmpty(() => homePage.FeaturedPublicationEyebrow, value => homePage.FeaturedPublicationEyebrow = value, "Featured Publication");
            hasChanges |= FillIfEmpty(() => homePage.LatestFeaturedLinkLabel, value => homePage.LatestFeaturedLinkLabel = value, "Read more ->");
            hasChanges |= FillIfEmpty(() => homePage.LatestIntelligenceLinkLabel, value => homePage.LatestIntelligenceLinkLabel = value, "Read more ->");
            hasChanges |= FillIfEmpty(() => homePage.AboutTitle, value => homePage.AboutTitle = value, "About CAFRI");
            hasChanges |= FillIfEmpty(() => homePage.AboutDescription, value => homePage.AboutDescription = value, "CAFRI is designed as a regional intelligence initiative focused on Central Asia. The platform helps users explore country profiles, investment climate, regulatory environment, publications and regional context through a clean, structured interface.");

            if (!homePage.FeaturedPublicationId.HasValue && featuredPublicationId.HasValue)
            {
                homePage.FeaturedPublicationId = featuredPublicationId;
                hasChanges = true;
            }

            if (string.IsNullOrWhiteSpace(homePage.DirectionsText))
            {
                homePage.DirectionsText = HomePageContentTextSerializer.JoinDirections(
                [
                    new() { Number = "01", Title = "Explore Countries", Description = "Access country-level profiles covering institutions, market environment, regulation and development priorities.", Url = "/countries", LinkLabel = "View country profiles ->" },
                    new() { Number = "02", Title = "Investment Climate", Description = "Understand investment conditions, sector priorities, infrastructure and regional trade corridors.", Url = "/about", LinkLabel = "Explore investment climate ->" },
                    new() { Number = "03", Title = "Regulatory Intelligence", Description = "Track regulatory developments, policy changes and institutional updates across Central Asia.", Url = "/intelligence", LinkLabel = "View updates ->" },
                    new() { Number = "04", Title = "Publications", Description = "Read research reports, policy briefs, country reviews and analytical materials.", Url = "/publications", LinkLabel = "View publications ->" },
                    new() { Number = "05", Title = "Regional Coverage", Description = "Navigate Central Asia through structured country profiles and explore regulation, investment climate and related publications.", Url = "/countries", LinkLabel = "Open country coverage ->", IsWide = true }
                ]);
                hasChanges = true;
            }

            if (string.IsNullOrWhiteSpace(homePage.FeatureCardsText))
            {
                homePage.FeatureCardsText = HomePageContentTextSerializer.JoinFeatureCards(
                [
                    new() { Title = "Investment Climate", Description = "Market conditions, sectors, infrastructure, business environment and regional corridors.", Url = "/about", CssClassName = "home-v2-feature-card--investment" },
                    new() { Title = "Regulatory Intelligence", Description = "Policy changes, institutional updates and regulation-focused monitoring.", Url = "/intelligence", CssClassName = "home-v2-feature-card--regulatory" },
                    new() { Title = "Publications", Description = "Reports, research notes, country reviews and analytical materials.", Url = "/publications", CssClassName = "home-v2-feature-card--publications" }
                ]);
                hasChanges = true;
            }

            if (string.IsNullOrWhiteSpace(homePage.CoverageAreasText))
            {
                homePage.CoverageAreasText = HomePageContentTextSerializer.JoinCoverageAreas(
                [
                    new() { Icon = "institution", Title = "Financial Regulation", Description = "Monitoring regulatory developments from central banks, financial supervisors, and government institutions." },
                    new() { Icon = "institution", Title = "Banking Intelligence", Description = "Analysis of monetary policy, banking sector performance, licensing, and supervisory actions." },
                    new() { Icon = "shield", Title = "Sanctions & Compliance", Description = "Tracking domestic and international sanctions, AML/CFT frameworks, and compliance requirements." },
                    new() { Icon = "truck", Title = "Trade & Logistics", Description = "Monitoring trade corridors, customs, infrastructure, and cross-border logistics developments." },
                    new() { Icon = "chart", Title = "Macroeconomics", Description = "Key economic indicators, forecasts, fiscal policy, and structural economic trends." },
                    new() { Icon = "globe", Title = "Geopolitical Risk", Description = "Analysis of political risk, regional dynamics, and their impact on financial and trade stability." }
                ]);
                hasChanges = true;
            }

            if (hasChanges)
            {
                homePage.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }
        }

        if (_dbContext.ChangeTracker.HasChanges())
        {
            await _dbContext.SaveChangesAsync();
        }
    }

    private static List<CountryOverviewCardViewModel> BuildCountries() =>
    [
        new() { Code = "KZ", Name = "Kazakhstan", Capital = "Astana", Summary = "Largest economy in Central Asia and a regional financial leader with active regulatory reforms.", Gdp = "$288.1B", Population = "20.3M", BankAssets = "$137.2B", Slug = "kazakhstan", HeroClassName = "country-card-image--kazakhstan", Metrics = [ new() { Icon = "chart", Label = "GDP (2025)", Value = "$288.1B" }, new() { Icon = "people", Label = "Population", Value = "20.3M" }, new() { Icon = "bank", Label = "Bank Assets", Value = "$137.2B" } ] },
        new() { Code = "UZ", Name = "Uzbekistan", Capital = "Tashkent", Summary = "Fast-growing economy with significant financial sector transformation and trade expansion.", Gdp = "$115.2B", Population = "36.7M", BankAssets = "$55.8B", Slug = "uzbekistan", HeroClassName = "country-card-image--uzbekistan", Metrics = [ new() { Icon = "chart", Label = "GDP (2025)", Value = "$115.2B" }, new() { Icon = "people", Label = "Population", Value = "36.7M" }, new() { Icon = "bank", Label = "Bank Assets", Value = "$55.8B" } ] },
        new() { Code = "KG", Name = "Kyrgyzstan", Capital = "Bishkek", Summary = "Open economy integrated into the EAEU with strong remittance inflows and regional connectivity.", Gdp = "$16.1B", Population = "6.9M", BankAssets = "$10.4B", Slug = "kyrgyzstan", HeroClassName = "country-card-image--kyrgyzstan", Metrics = [ new() { Icon = "chart", Label = "GDP (2025)", Value = "$16.1B" }, new() { Icon = "people", Label = "Population", Value = "6.9M" }, new() { Icon = "bank", Label = "Bank Assets", Value = "$10.4B" } ] },
        new() { Code = "TJ", Name = "Tajikistan", Capital = "Dushanbe", Summary = "Remittance-dependent economy with evolving banking regulation and infrastructure development.", Gdp = "$10.5B", Population = "10.5M", BankAssets = "$6.7B", Slug = "tajikistan", HeroClassName = "country-card-image--tajikistan", Metrics = [ new() { Icon = "chart", Label = "GDP (2025)", Value = "$10.5B" }, new() { Icon = "people", Label = "Population", Value = "10.5M" }, new() { Icon = "bank", Label = "Bank Assets", Value = "$6.7B" } ] },
        new() { Code = "TM", Name = "Turkmenistan", Capital = "Ashgabat", Summary = "Energy-rich economy with state-dominated financial system and strategic regional initiatives.", Gdp = "$45.9B", Population = "6.4M", BankAssets = "$8.1B", Slug = "turkmenistan", HeroClassName = "country-card-image--turkmenistan", Metrics = [ new() { Icon = "chart", Label = "GDP (2025)", Value = "$45.9B" }, new() { Icon = "people", Label = "Population", Value = "6.4M" }, new() { Icon = "bank", Label = "Bank Assets", Value = "$8.1B" } ] }
    ];

    private static CountryProfilePageViewModel BuildCountryProfile(CountryOverviewCardViewModel country)
    {
        var isKazakhstan = string.Equals(country.Slug, "kazakhstan", StringComparison.OrdinalIgnoreCase);
        return new CountryProfilePageViewModel
        {
            Country = country,
            HeroDescription = isKazakhstan ? "Comprehensive financial, regulatory and macroeconomic profile of Kazakhstan, the largest economy in Central Asia and a key regional financial hub." : $"Comprehensive financial, regulatory and macroeconomic profile of {country.Name}, with focus on banking sector dynamics, institutions, and regional connectivity.",
            Currency = country.Code switch { "UZ" => "Uzbekistani Som (UZS)", "KG" => "Kyrgyzstani Som (KGS)", "TJ" => "Tajikistani Somoni (TJS)", "TM" => "Turkmenistan Manat (TMT)", _ => "Kazakhstani Tenge (KZT)" },
            RiskRating = country.Code switch { "UZ" => "BB", "KG" => "B+", "TJ" => "B", "TM" => "B", _ => "BB+" },
            RiskOutlook = country.Code switch { "TJ" => "Positive Outlook", _ => "Stable Outlook" },
            DoingBusinessRank = country.Code switch { "UZ" => "69 / 190", "KG" => "80 / 190", "TJ" => "106 / 190", "TM" => "Unavailable", _ => "25 / 190" },
            DoingBusinessSource = country.Code switch { "TM" => "Legacy benchmark", _ => "World Bank" },
            OverviewText = $"{country.Name} has a diversified, increasingly market-oriented economy with an evolving financial sector and a steadily expanding regulatory framework. The country remains a strategic node for regional trade, capital formation, and institutional modernization across Central Asia.",
            HeroIndicators = [ new() { Icon = "capital", Label = "Capital", Value = country.Capital }, new() { Icon = "population", Label = "Population (2025)", Value = country.Population }, new() { Icon = "chart", Label = "GDP (2025)", Value = country.Gdp }, new() { Icon = "currency", Label = "Currency", Value = country.Code switch { "UZ" => "Uzbekistani Som (UZS)", "KG" => "Kyrgyzstani Som (KGS)", "TJ" => "Tajikistani Somoni (TJS)", "TM" => "Turkmenistan Manat (TMT)", _ => "Kazakhstani Tenge (KZT)" } } ],
            KeyIndicators = [ new() { Label = "Real GDP Growth", Value = country.Code == "KZ" ? "4.8%" : "5.1%" }, new() { Label = "Inflation (Avg.)", Value = country.Code == "KZ" ? "8.4%" : "7.9%" }, new() { Label = "Current Account / GDP", Value = country.Code == "KZ" ? "-1.7%" : "-2.1%" }, new() { Label = "FX Reserves", Value = country.Code == "KZ" ? "$42.7B" : "$17.5B" }, new() { Label = "Public Debt / GDP", Value = country.Code == "KZ" ? "18.7%" : "28.4%" } ],
            Tabs = [ new() { Label = "Overview", IsActive = true }, new() { Label = "Financial Sector" }, new() { Label = "Regulatory Framework" }, new() { Label = "Banking System" }, new() { Label = "Capital Markets" }, new() { Label = "Insurance" }, new() { Label = "Payments" }, new() { Label = "Trade & Logistics" }, new() { Label = "Risk & Compliance" }, new() { Label = "Data & Statistics" } ],
            Highlights = [ new() { Icon = "chart", Title = "Largest Economy in Central Asia", Description = "Strong diversification and regional financial leadership." }, new() { Icon = "shield", Title = "Investment Profile", Description = country.Code == "KZ" ? "Investment grade benchmarks and active reform momentum." : "Improving reform environment with targeted sector modernization." }, new() { Icon = "trade", Title = "Regional Connectivity", Description = "EAEU, WTO, SCO, and corridor integration shape cross-border flows." }, new() { Icon = "bank", Title = "Banking Sector Stability", Description = "Gradual balance-sheet strengthening and tighter supervision." } ],
            MacroSummaryCards = [ new() { Icon = "chart", Label = "GDP (Nominal, 2025)", Value = country.Gdp, Description = country.Code == "KZ" ? "+4.8% YoY" : "+5.1% YoY" }, new() { Icon = "chart", Label = "GDP per Capita (2025)", Value = country.Code == "KZ" ? "$14,190" : "$3,140", Description = country.Code == "KZ" ? "+3.2% YoY" : "+4.0% YoY" }, new() { Icon = "trade", Label = "Total Trade (2025)", Value = country.Code == "KZ" ? "$139.2B" : "$41.8B", Description = country.Code == "KZ" ? "+6.1% YoY" : "+7.2% YoY" } ],
            MacroIndicatorRows = [ new() { Indicator = "Real GDP Growth (%)", Value2023 = "5.1", Value2024 = "4.3", Value2025 = "4.8", Value2026 = "4.5" }, new() { Indicator = "Inflation, Avg. (%)", Value2023 = "9.8", Value2024 = "8.6", Value2025 = "8.4", Value2026 = "7.5" }, new() { Indicator = "Current Account / GDP (%)", Value2023 = "-2.1", Value2024 = "-1.9", Value2025 = "-1.7", Value2026 = "-1.6" }, new() { Indicator = "Fiscal Balance / GDP (%)", Value2023 = "-2.3", Value2024 = "-2.1", Value2025 = "-2.0", Value2026 = "-1.8" }, new() { Indicator = "Public Debt / GDP (%)", Value2023 = "16.9", Value2024 = "17.8", Value2025 = "18.7", Value2026 = "19.2" }, new() { Indicator = "FX Reserves (USD B)", Value2023 = "$33.6B", Value2024 = "$38.7B", Value2025 = "$42.7B", Value2026 = "$45.5B" } ],
            GdpGrowthChart = new() { Title = "Real GDP Growth (%)", ValueSuffix = "%", Labels = ["2021", "2022", "2023", "2024", "2025*", "2026*"], Values = [4.1m, 3.2m, 5.1m, 4.3m, 4.8m, 4.5m] },
            InflationChart = new() { Title = "Inflation, Avg. (%)", ValueSuffix = "%", Labels = ["2021", "2022", "2023", "2024", "2025*", "2026*"], Values = [7.9m, 14.5m, 9.8m, 8.6m, 8.4m, 7.5m] },
            FinancialSectorSnapshot = [ new() { Icon = "bank", Label = "Banks", Value = country.Code == "KZ" ? "21" : "27", Description = "Active" }, new() { Icon = "chart", Label = "Total Assets", Value = country.BankAssets, Description = "+6.3% YoY" }, new() { Icon = "shield", Label = "Capital Adequacy", Value = country.Code == "KZ" ? "18.7%" : "17.1%", Description = "Well Capitalized" }, new() { Icon = "check", Label = "Non-Performing Loans", Value = country.Code == "KZ" ? "3.2%" : "4.8%", Description = "Improving" } ],
            BankingAssetsChart = new() { Title = "Banking Sector Assets (USD Billion)", ValueSuffix = "B", Labels = ["2021", "2022", "2023", "2024", "2025*"], Values = country.Code == "KZ" ? [89.3m, 95.6m, 110.2m, 128.9m, 137.2m] : [24.5m, 27.8m, 34.2m, 47.1m, 55.8m] },
            TopBanks = [ new() { Name = country.Code == "KZ" ? "Halyk Bank" : "National Bank for Foreign Economic Activity", Assets = country.Code == "KZ" ? "32.1" : "18.6", MarketShare = country.Code == "KZ" ? "23.4%" : "19.1%" }, new() { Name = country.Code == "KZ" ? "Kaspi Bank" : "SQB", Assets = country.Code == "KZ" ? "24.7" : "9.8", MarketShare = country.Code == "KZ" ? "18.0%" : "10.1%" }, new() { Name = country.Code == "KZ" ? "ForteBank" : "Asaka Bank", Assets = country.Code == "KZ" ? "12.3" : "7.5", MarketShare = country.Code == "KZ" ? "9.0%" : "7.7%" }, new() { Name = country.Code == "KZ" ? "Jusan Bank" : "Ipoteka Bank", Assets = country.Code == "KZ" ? "8.7" : "6.9", MarketShare = country.Code == "KZ" ? "6.4%" : "7.1%" }, new() { Name = country.Code == "KZ" ? "Eurasian Bank" : "Agrobank", Assets = country.Code == "KZ" ? "6.2" : "5.8", MarketShare = country.Code == "KZ" ? "4.5%" : "6.0%" } ],
            LatestUpdates = [ new() { Title = country.Code == "KZ" ? "National Bank maintains base rate at 16.75%" : $"{country.Name} central bank updates monetary stance", Meta = "Jun 17, 2026" }, new() { Title = "New AML/CFT regulatory guidelines issued", Meta = "Jun 10, 2026" }, new() { Title = $"{country.Name} completes financial regulation update", Meta = "May 28, 2026" } ],
            RelatedPublications = [ new() { Title = $"{country.Name} Regulatory Landscape Q2 2026", Meta = "Regulatory Note | Jun 2026" }, new() { Title = $"{country.Name} Banking Sector Review 2026", Meta = "Analytical Brief | May 2026" }, new() { Title = $"{country.Name}: Financial Services Hub Developments", Meta = "Report | Apr 2026" } ],
            KeyInstitutions = [ new() { Name = country.Code == "KZ" ? "National Bank of Kazakhstan" : $"{country.Name} Central Bank", Role = "Central Bank" }, new() { Name = "Ministry of Finance", Role = "Government" }, new() { Name = country.Code == "KZ" ? "Agency for Regulation and Development of Financial Market" : "Financial Market Regulator", Role = "Regulator" }, new() { Name = country.Code == "KZ" ? "AIFC Authority" : "Investment Promotion Agency", Role = "Financial Services Regulator" } ],
            ExploreLinks = [ new() { Title = "Regulatory Framework", Description = "View regulations and laws", Url = "#" }, new() { Title = "Banking System", Description = "Detailed banking sector data", Url = "#" }, new() { Title = "Capital Markets", Description = "Market overview and statistics", Url = "#" }, new() { Title = "Payments & Fintech", Description = "Payment systems and fintech", Url = "#" }, new() { Title = "Trade & Logistics", Description = "Corridors and trade data", Url = "#" } ]
        };
    }

    private static List<(string Name, string Slug, string Description, string Scope)> BuildCategories() =>
    [
        ("Financial Regulation", "financial-regulation", "Core regulatory and supervisory developments affecting banks, markets, and compliance.", "Intelligence"),
        ("Banking", "banking", "Banking-sector structure, reforms, capital, liquidity and market positioning.", "Shared"),
        ("Trade & Logistics", "trade-logistics", "Trade routes, customs, corridor infrastructure and logistics intelligence.", "Shared"),
        ("Sanctions Analysis", "sanctions-analysis", "Sanctions exposure, compliance obligations and risk developments.", "Publications"),
        ("Macroeconomics", "macroeconomics", "Growth, inflation, fiscal, external balance and sovereign context.", "Intelligence"),
        ("Country Profile", "country-profile", "Long-form country reference materials and institutional snapshots.", "Publications")
    ];

    private static List<(string Name, string Slug, string SourceType, string CountryCode, string Url, string Summary)> BuildSources() =>
    [
        ("National Bank of Kazakhstan", "national-bank-kazakhstan", "Central Bank", "KZ", "https://www.nationalbank.kz", "Primary source for monetary policy, FX regulation and banking statistics in Kazakhstan."),
        ("Central Bank of Uzbekistan", "central-bank-uzbekistan", "Central Bank", "UZ", "https://cbu.uz", "Official source for monetary, banking and regulatory updates in Uzbekistan."),
        ("Agency for Regulation and Development of the Financial Market", "arrrf-kazakhstan", "Financial Regulator", "KZ", "https://www.gov.kz/memleket/entities/ardfm", "Supervisory and prudential authority for Kazakhstan's financial markets."),
        ("Eurasian Development Bank", "eurasian-development-bank", "IFI", "Regional", "https://eabr.org", "Regional development bank source for macro, corridor and integration analytics."),
        ("World Bank Data", "world-bank-data", "Multilateral", "Regional", "https://data.worldbank.org", "Benchmark macroeconomic and development indicators across Central Asia.")
    ];

    private static List<(string CountryCode, string Name, string Slug, string Category, string Unit, string LatestValue, string YearLabel, string SourceName, string Notes)> BuildIndicators() =>
    [
        ("KZ", "Real GDP Growth", "kz-real-gdp-growth-2023", "Macroeconomics", "%", "5.1", "2023", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("KZ", "Real GDP Growth", "kz-real-gdp-growth-2024", "Macroeconomics", "%", "4.1", "2024", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("KZ", "Real GDP Growth", "kz-real-gdp-growth-2025", "Macroeconomics", "%", "4.8", "2025", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("KZ", "Banking Sector Assets", "kz-banking-assets", "Banking", "USD B", "137.2", "2025", "National Bank of Kazakhstan", "Aggregated banking assets."),
        ("UZ", "Real GDP Growth", "uz-real-gdp-growth-2023", "Macroeconomics", "%", "6.0", "2023", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("UZ", "Real GDP Growth", "uz-real-gdp-growth-2024", "Macroeconomics", "%", "6.5", "2024", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("UZ", "Real GDP Growth", "uz-real-gdp-growth-2025", "Macroeconomics", "%", "6.8", "2025", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("UZ", "Banking Sector Assets", "uz-banking-assets", "Banking", "USD B", "55.8", "2025", "Central Bank of Uzbekistan", "Aggregated banking assets."),
        ("KG", "Real GDP Growth", "kg-real-gdp-growth-2023", "Macroeconomics", "%", "6.2", "2023", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("KG", "Real GDP Growth", "kg-real-gdp-growth-2024", "Macroeconomics", "%", "7.0", "2024", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("KG", "Real GDP Growth", "kg-real-gdp-growth-2025", "Macroeconomics", "%", "8.0", "2025", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("KG", "Population", "kg-population", "Demographics", "M", "6.9", "2025", "World Bank Data", "Resident population estimate."),
        ("TJ", "Real GDP Growth", "tj-real-gdp-growth-2023", "Macroeconomics", "%", "8.0", "2023", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("TJ", "Real GDP Growth", "tj-real-gdp-growth-2024", "Macroeconomics", "%", "8.3", "2024", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("TJ", "Real GDP Growth", "tj-real-gdp-growth-2025", "Macroeconomics", "%", "8.2", "2025", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("TJ", "Inflation", "tj-inflation", "Macroeconomics", "%", "6.1", "2025", "World Bank Data", "Average annual CPI estimate."),
        ("TM", "Real GDP Growth", "tm-real-gdp-growth-2023", "Macroeconomics", "%", "2.3", "2023", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("TM", "Real GDP Growth", "tm-real-gdp-growth-2024", "Macroeconomics", "%", "2.5", "2024", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("TM", "Real GDP Growth", "tm-real-gdp-growth-2025", "Macroeconomics", "%", "2.6", "2025", "World Bank Data", "Illustrative value for editorial and admin workflows."),
        ("TM", "GDP (Nominal)", "tm-gdp-nominal", "Macroeconomics", "USD B", "45.9", "2025", "World Bank Data", "Nominal GDP estimate.")
    ];

    private static bool FillIfEmpty(Func<string> getter, Action<string> setter, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(getter()))
        {
            return false;
        }

        setter(fallback);
        return true;
    }
}
