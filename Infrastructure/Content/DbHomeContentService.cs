using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Home;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public sealed class DbHomeContentService : IHomeContentService
{
    private readonly AppDbContext _dbContext;

    public DbHomeContentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HomePageViewModel> GetHomePageAsync(CancellationToken cancellationToken = default)
    {
        var page = await _dbContext.HomePageContents
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (page is null)
        {
            return BuildFallbackHomePage();
        }

        var latestIntelligence = await _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ThenBy(x => x.DisplayOrder)
            .Take(2)
            .Select(x => new HomeIntelligenceCardViewModel
            {
                Category = x.Category,
                CountryCode = x.CountryCode,
                PublishedLabel = x.PublishedLabel,
                Title = x.Title,
                Summary = x.Description,
                Source = x.Source,
                Url = $"/intelligence/{x.Slug}"
            })
            .ToListAsync(cancellationToken);

        var featuredPublicationEntity = await _dbContext.PublicationContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished && (!page.FeaturedPublicationId.HasValue || x.Id == page.FeaturedPublicationId.Value))
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ThenBy(x => x.DisplayOrder)
            .FirstOrDefaultAsync(cancellationToken);

        if (featuredPublicationEntity is null)
        {
            featuredPublicationEntity = await _dbContext.PublicationContentItems
                .AsNoTracking()
                .Where(x => x.IsPublished)
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenBy(x => x.DisplayOrder)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return new HomePageViewModel
        {
            HeroTitle = page.HeroTitle,
            HeroLead = page.HeroLead,
            HeroPrimaryCtaLabel = page.HeroPrimaryCtaLabel,
            HeroPrimaryCtaUrl = page.HeroPrimaryCtaUrl,
            HeroSecondaryCtaLabel = page.HeroSecondaryCtaLabel,
            HeroSecondaryCtaUrl = page.HeroSecondaryCtaUrl,
            IntroTitle = page.IntroTitle,
            IntroDescription = page.IntroDescription,
            PlatformSectionLabel = page.PlatformSectionLabel,
            PlatformSectionTitle = page.PlatformSectionTitle,
            PlatformSectionDescription = page.PlatformSectionDescription,
            Directions = HomePageContentTextSerializer.ParseDirections(page.DirectionsText),
            CountriesSectionLabel = page.CountriesSectionLabel,
            CountriesSectionTitle = page.CountriesSectionTitle,
            CountriesSectionDescription = page.CountriesSectionDescription,
            CountriesVisualTitle = page.CountriesVisualTitle,
            CountriesVisualDescription = page.CountriesVisualDescription,
            FeaturesSectionLabel = page.FeaturesSectionLabel,
            FeaturesSectionTitle = page.FeaturesSectionTitle,
            FeaturesSectionDescription = page.FeaturesSectionDescription,
            FeatureCards = HomePageContentTextSerializer.ParseFeatureCards(page.FeatureCardsText),
            LatestSectionLabel = page.LatestSectionLabel,
            LatestSectionTitle = page.LatestSectionTitle,
            LatestSectionDescription = page.LatestSectionDescription,
            FeaturedPublicationEyebrow = page.FeaturedPublicationEyebrow,
            LatestFeaturedLinkLabel = page.LatestFeaturedLinkLabel,
            LatestIntelligenceLinkLabel = page.LatestIntelligenceLinkLabel,
            AboutTitle = page.AboutTitle,
            AboutDescription = page.AboutDescription,
            LatestIntelligence = latestIntelligence,
            FeaturedPublication = featuredPublicationEntity is null ? new HomeFeaturedPublicationViewModel() : MapFeaturedPublication(featuredPublicationEntity),
            CoverageAreas = HomePageContentTextSerializer.ParseCoverageAreas(page.CoverageAreasText)
        };
    }

    private static HomeFeaturedPublicationViewModel MapFeaturedPublication(Domain.Content.PublicationContentItem item) =>
        new()
        {
            Eyebrow = "Featured Publication",
            Title = item.Title,
            Subtitle = item.Type,
            Description = item.Description,
            ReadOnlineUrl = $"/publications/{item.Slug}",
            DownloadUrl = item.PdfDownloadUrl ?? "#",
            MetaItems =
            [
                new() { Icon = "document", Label = item.Type },
                new() { Icon = "calendar", Label = item.PublishedDate },
                new() { Icon = "clock", Label = item.ReadingTime },
                new() { Icon = "user", Label = item.AuthorLabel }
            ]
        };

    private static HomePageViewModel BuildFallbackHomePage() =>
        new()
        {
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
            Directions = HomePageContentTextSerializer.ParseDirections(
                "01 | Explore Countries | Access country-level profiles covering institutions, market environment, regulation and development priorities. | /countries | View country profiles ->\n" +
                "02 | Investment Climate | Understand investment conditions, sector priorities, infrastructure and regional trade corridors. | /about | Explore investment climate ->\n" +
                "03 | Regulatory Intelligence | Track regulatory developments, policy changes and institutional updates across Central Asia. | /intelligence | View updates ->\n" +
                "04 | Publications | Read research reports, policy briefs, country reviews and analytical materials. | /publications | View publications ->\n" +
                "05 | Regional Coverage | Navigate Central Asia through structured country profiles and explore regulation, investment climate and related publications. | /countries | Open country coverage -> | wide"),
            CountriesSectionLabel = "Explore countries",
            CountriesSectionTitle = "Country profiles across Central Asia",
            CountriesSectionDescription = "Each country page includes overview, institutions, regulation, investment environment, infrastructure and related publications.",
            CountriesVisualTitle = "Regional Country Coverage",
            CountriesVisualDescription = "A structured entry point to Central Asia. Open a country profile to access its regulatory environment, investment climate and publications.",
            FeaturesSectionLabel = "Core themes",
            FeaturesSectionTitle = "Investment climate, regulation and research",
            FeaturesSectionDescription = "Three core content pillars help users understand the region through a clear, structured and institutional-grade interface.",
            FeatureCards = HomePageContentTextSerializer.ParseFeatureCards(
                "Investment Climate | Market conditions, sectors, infrastructure, business environment and regional corridors. | /about | home-v2-feature-card--investment\n" +
                "Regulatory Intelligence | Policy changes, institutional updates and regulation-focused monitoring. | /intelligence | home-v2-feature-card--regulatory\n" +
                "Publications | Reports, research notes, country reviews and analytical materials. | /publications | home-v2-feature-card--publications"),
            LatestSectionLabel = "Latest materials",
            LatestSectionTitle = "Latest intelligence and publications",
            LatestSectionDescription = "The latest updates added through CAFRI content workflows: intelligence items, reports, briefs and regional analytical materials.",
            FeaturedPublicationEyebrow = "Featured Publication",
            LatestFeaturedLinkLabel = "Read more ->",
            LatestIntelligenceLinkLabel = "Read more ->",
            AboutTitle = "About CAFRI",
            AboutDescription = "CAFRI is designed as a regional intelligence initiative focused on Central Asia. The platform helps users explore country profiles, investment climate, regulatory environment, publications and regional context through a clean, structured interface.",
            CoverageAreas = HomePageContentTextSerializer.ParseCoverageAreas(
                "institution | Financial Regulation | Monitoring regulatory developments from central banks, financial supervisors, and government institutions.\n" +
                "institution | Banking Intelligence | Analysis of monetary policy, banking sector performance, licensing, and supervisory actions.\n" +
                "shield | Sanctions & Compliance | Tracking domestic and international sanctions, AML/CFT frameworks, and compliance requirements.\n" +
                "truck | Trade & Logistics | Monitoring trade corridors, customs, infrastructure, and cross-border logistics developments.\n" +
                "chart | Macroeconomics | Key economic indicators, forecasts, fiscal policy, and structural economic trends.\n" +
                "globe | Geopolitical Risk | Analysis of political risk, regional dynamics, and their impact on financial and trade stability.")
        };
}
