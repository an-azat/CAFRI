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
                    PublishedAtUtc = PublicationMetadataFormatter.ParsePublishedDate(item.PublishedLabel),
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
                    PublishedAtUtc = PublicationMetadataFormatter.ParsePublishedDateFromMeta(item.Meta),
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
                // Profile content comes from the end-2025 reference data; BuildCountries() only
                // supplies the identity (name, slug, card style).
                var facts = CountryReferenceData2025.Countries.First(x => x.Code == country.Code);
                _dbContext.CountryContents.Add(new CountryContent
                {
                    Id = Guid.NewGuid(),
                    Code = country.Code,
                    Slug = country.Slug,
                    Name = country.Name,
                    OfficialName = facts.OfficialName,
                    Region = "Central Asia",
                    Capital = facts.Capital,
                    Summary = facts.Summary,
                    ProfileLead = facts.ProfileLead,
                    AboutText = facts.AboutText,
                    Currency = facts.Currency,
                    OfficialLanguages = facts.OfficialLanguages,
                    Area = facts.Area,
                    NeighbouringCountries = facts.NeighbouringCountries,
                    Population = facts.Population,
                    PopulationPeriod = CountryReferenceData2025.PopulationPeriod,
                    Gdp = facts.Gdp,
                    GdpPeriod = CountryReferenceData2025.GdpPeriod,
                    DataSource = CountryReferenceData2025.DataSource,
                    BankAssets = string.Empty,
                    ProfileJson = JsonSerializer.Serialize(CountryReferenceData2025.BuildProfile(country.Code)),
                    HeroClassName = country.HeroClassName,
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

    // End-2025 indicators shared with the ReplaceCountryDataWithEnd2025 migration.
    private static List<(string CountryCode, string Name, string Slug, string Category, string Unit, string LatestValue, string YearLabel, string SourceName, string Notes)> BuildIndicators() =>
        CountryReferenceData2025.BuildIndicators()
            .Select(x => (x.CountryCode, x.Name, x.Slug, x.Category, x.Unit, x.LatestValue, x.YearLabel, x.SourceName, x.Notes))
            .ToList();

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
