using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedHomePageContentData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "HomePageContents" (
                    "Id",
                    "HeroTitle",
                    "HeroLead",
                    "HeroPrimaryCtaLabel",
                    "HeroPrimaryCtaUrl",
                    "HeroSecondaryCtaLabel",
                    "HeroSecondaryCtaUrl",
                    "IntroTitle",
                    "IntroDescription",
                    "PlatformSectionLabel",
                    "PlatformSectionTitle",
                    "PlatformSectionDescription",
                    "DirectionsText",
                    "CountriesSectionLabel",
                    "CountriesSectionTitle",
                    "CountriesSectionDescription",
                    "CountriesVisualTitle",
                    "CountriesVisualDescription",
                    "FeaturesSectionLabel",
                    "FeaturesSectionTitle",
                    "FeaturesSectionDescription",
                    "FeatureCardsText",
                    "LatestSectionLabel",
                    "LatestSectionTitle",
                    "LatestSectionDescription",
                    "FeaturedPublicationEyebrow",
                    "LatestFeaturedLinkLabel",
                    "LatestIntelligenceLinkLabel",
                    "FeaturedPublicationId",
                    "AboutTitle",
                    "AboutDescription",
                    "CoverageAreasText",
                    "CreatedAtUtc",
                    "UpdatedAtUtc"
                )
                SELECT
                    '8a34e3b0-cab9-4a5a-a3f1-f7be2a336001'::uuid,
                    'Central Asia Financial & Regulatory Intelligence Initiative',
                    'Your gateway to country profiles, investment climate, regulatory developments, publications and regional intelligence across Central Asia.',
                    'Explore Countries ->',
                    '/countries',
                    'View Intelligence',
                    '/intelligence',
                    'One regional platform for Central Asia',
                    'CAFRI brings together structured country information, investment climate context, regulatory intelligence, publications and an interactive regional map in one place. The platform is built for investors, researchers, policy analysts and institutions.',
                    'Platform sections',
                    'What you can explore',
                    'Main entry points of the platform: countries, investment environment, regulatory monitoring, publications and the regional map.',
                    E'01 | Explore Countries | Access country-level profiles covering institutions, market environment, regulation and development priorities. | /countries | View country profiles ->\n02 | Investment Climate | Understand investment conditions, sector priorities, infrastructure and regional trade corridors. | /about | Explore investment climate ->\n03 | Regulatory Intelligence | Track regulatory developments, policy changes and institutional updates across Central Asia. | /intelligence | View updates ->\n04 | Publications | Read research reports, policy briefs, country reviews and analytical materials. | /publications | View publications ->\n05 | Regional Coverage | Navigate Central Asia through structured country profiles and explore regulation, investment climate and related publications. | /countries | Open country coverage -> | wide',
                    'Explore countries',
                    'Country profiles across Central Asia',
                    'Each country page includes overview, institutions, regulation, investment environment, infrastructure and related publications.',
                    'Regional Country Coverage',
                    'A structured entry point to Central Asia. Open a country profile to access its regulatory environment, investment climate and publications.',
                    'Core themes',
                    'Investment climate, regulation and research',
                    'Three core content pillars help users understand the region through a clear, structured and institutional-grade interface.',
                    E'Investment Climate | Market conditions, sectors, infrastructure, business environment and regional corridors. | /about | home-v2-feature-card--investment\nRegulatory Intelligence | Policy changes, institutional updates and regulation-focused monitoring. | /intelligence | home-v2-feature-card--regulatory\nPublications | Reports, research notes, country reviews and analytical materials. | /publications | home-v2-feature-card--publications',
                    'Latest materials',
                    'Latest intelligence and publications',
                    'The latest updates added through CAFRI content workflows: intelligence items, reports, briefs and regional analytical materials.',
                    'Featured Publication',
                    'Read more ->',
                    'Read more ->',
                    (
                        SELECT "Id"
                        FROM "PublicationContentItems"
                        WHERE "Slug" = 'tajikistan-banking-sector-outlook'
                        ORDER BY "UpdatedAtUtc" DESC
                        LIMIT 1
                    ),
                    'About CAFRI',
                    'CAFRI is designed as a regional intelligence initiative focused on Central Asia. The platform helps users explore country profiles, investment climate, regulatory environment, publications and regional context through a clean, structured interface.',
                    E'institution | Financial Regulation | Monitoring regulatory developments from central banks, financial supervisors, and government institutions.\ninstitution | Banking Intelligence | Analysis of monetary policy, banking sector performance, licensing, and supervisory actions.\nshield | Sanctions & Compliance | Tracking domestic and international sanctions, AML/CFT frameworks, and compliance requirements.\ntruck | Trade & Logistics | Monitoring trade corridors, customs, infrastructure, and cross-border logistics developments.\nchart | Macroeconomics | Key economic indicators, forecasts, fiscal policy, and structural economic trends.\nglobe | Geopolitical Risk | Analysis of political risk, regional dynamics, and their impact on financial and trade stability.',
                    TIMESTAMPTZ '2026-08-03 00:00:00+00',
                    TIMESTAMPTZ '2026-08-03 00:00:00+00'
                WHERE NOT EXISTS (
                    SELECT 1 FROM "HomePageContents"
                );
                """);

            migrationBuilder.Sql(
                """
                UPDATE "HomePageContents"
                SET
                    "HeroTitle" = COALESCE(NULLIF("HeroTitle", ''), 'Central Asia Financial & Regulatory Intelligence Initiative'),
                    "HeroLead" = COALESCE(NULLIF("HeroLead", ''), 'Your gateway to country profiles, investment climate, regulatory developments, publications and regional intelligence across Central Asia.'),
                    "HeroPrimaryCtaLabel" = COALESCE(NULLIF("HeroPrimaryCtaLabel", ''), 'Explore Countries ->'),
                    "HeroPrimaryCtaUrl" = COALESCE(NULLIF("HeroPrimaryCtaUrl", ''), '/countries'),
                    "HeroSecondaryCtaLabel" = COALESCE(NULLIF("HeroSecondaryCtaLabel", ''), 'View Intelligence'),
                    "HeroSecondaryCtaUrl" = COALESCE(NULLIF("HeroSecondaryCtaUrl", ''), '/intelligence'),
                    "IntroTitle" = COALESCE(NULLIF("IntroTitle", ''), 'One regional platform for Central Asia'),
                    "IntroDescription" = COALESCE(NULLIF("IntroDescription", ''), 'CAFRI brings together structured country information, investment climate context, regulatory intelligence, publications and an interactive regional map in one place. The platform is built for investors, researchers, policy analysts and institutions.'),
                    "PlatformSectionLabel" = COALESCE(NULLIF("PlatformSectionLabel", ''), 'Platform sections'),
                    "PlatformSectionTitle" = COALESCE(NULLIF("PlatformSectionTitle", ''), 'What you can explore'),
                    "PlatformSectionDescription" = COALESCE(NULLIF("PlatformSectionDescription", ''), 'Main entry points of the platform: countries, investment environment, regulatory monitoring, publications and the regional map.'),
                    "DirectionsText" = COALESCE(NULLIF("DirectionsText", ''), E'01 | Explore Countries | Access country-level profiles covering institutions, market environment, regulation and development priorities. | /countries | View country profiles ->\n02 | Investment Climate | Understand investment conditions, sector priorities, infrastructure and regional trade corridors. | /about | Explore investment climate ->\n03 | Regulatory Intelligence | Track regulatory developments, policy changes and institutional updates across Central Asia. | /intelligence | View updates ->\n04 | Publications | Read research reports, policy briefs, country reviews and analytical materials. | /publications | View publications ->\n05 | Regional Coverage | Navigate Central Asia through structured country profiles and explore regulation, investment climate and related publications. | /countries | Open country coverage -> | wide'),
                    "CountriesSectionLabel" = COALESCE(NULLIF("CountriesSectionLabel", ''), 'Explore countries'),
                    "CountriesSectionTitle" = COALESCE(NULLIF("CountriesSectionTitle", ''), 'Country profiles across Central Asia'),
                    "CountriesSectionDescription" = COALESCE(NULLIF("CountriesSectionDescription", ''), 'Each country page includes overview, institutions, regulation, investment environment, infrastructure and related publications.'),
                    "CountriesVisualTitle" = COALESCE(NULLIF("CountriesVisualTitle", ''), 'Regional Country Coverage'),
                    "CountriesVisualDescription" = COALESCE(NULLIF("CountriesVisualDescription", ''), 'A structured entry point to Central Asia. Open a country profile to access its regulatory environment, investment climate and publications.'),
                    "FeaturesSectionLabel" = COALESCE(NULLIF("FeaturesSectionLabel", ''), 'Core themes'),
                    "FeaturesSectionTitle" = COALESCE(NULLIF("FeaturesSectionTitle", ''), 'Investment climate, regulation and research'),
                    "FeaturesSectionDescription" = COALESCE(NULLIF("FeaturesSectionDescription", ''), 'Three core content pillars help users understand the region through a clear, structured and institutional-grade interface.'),
                    "FeatureCardsText" = COALESCE(NULLIF("FeatureCardsText", ''), E'Investment Climate | Market conditions, sectors, infrastructure, business environment and regional corridors. | /about | home-v2-feature-card--investment\nRegulatory Intelligence | Policy changes, institutional updates and regulation-focused monitoring. | /intelligence | home-v2-feature-card--regulatory\nPublications | Reports, research notes, country reviews and analytical materials. | /publications | home-v2-feature-card--publications'),
                    "LatestSectionLabel" = COALESCE(NULLIF("LatestSectionLabel", ''), 'Latest materials'),
                    "LatestSectionTitle" = COALESCE(NULLIF("LatestSectionTitle", ''), 'Latest intelligence and publications'),
                    "LatestSectionDescription" = COALESCE(NULLIF("LatestSectionDescription", ''), 'The latest updates added through CAFRI content workflows: intelligence items, reports, briefs and regional analytical materials.'),
                    "FeaturedPublicationEyebrow" = COALESCE(NULLIF("FeaturedPublicationEyebrow", ''), 'Featured Publication'),
                    "LatestFeaturedLinkLabel" = COALESCE(NULLIF("LatestFeaturedLinkLabel", ''), 'Read more ->'),
                    "LatestIntelligenceLinkLabel" = COALESCE(NULLIF("LatestIntelligenceLinkLabel", ''), 'Read more ->'),
                    "FeaturedPublicationId" = COALESCE(
                        "FeaturedPublicationId",
                        (
                            SELECT "Id"
                            FROM "PublicationContentItems"
                            WHERE "Slug" = 'tajikistan-banking-sector-outlook'
                            ORDER BY "UpdatedAtUtc" DESC
                            LIMIT 1
                        )
                    ),
                    "AboutTitle" = COALESCE(NULLIF("AboutTitle", ''), 'About CAFRI'),
                    "AboutDescription" = COALESCE(NULLIF("AboutDescription", ''), 'CAFRI is designed as a regional intelligence initiative focused on Central Asia. The platform helps users explore country profiles, investment climate, regulatory environment, publications and regional context through a clean, structured interface.'),
                    "CoverageAreasText" = COALESCE(NULLIF("CoverageAreasText", ''), E'institution | Financial Regulation | Monitoring regulatory developments from central banks, financial supervisors, and government institutions.\ninstitution | Banking Intelligence | Analysis of monetary policy, banking sector performance, licensing, and supervisory actions.\nshield | Sanctions & Compliance | Tracking domestic and international sanctions, AML/CFT frameworks, and compliance requirements.\ntruck | Trade & Logistics | Monitoring trade corridors, customs, infrastructure, and cross-border logistics developments.\nchart | Macroeconomics | Key economic indicators, forecasts, fiscal policy, and structural economic trends.\nglobe | Geopolitical Risk | Analysis of political risk, regional dynamics, and their impact on financial and trade stability.'),
                    "UpdatedAtUtc" = NOW()
                WHERE
                    "HeroTitle" = '' OR
                    "HeroLead" = '' OR
                    "HeroPrimaryCtaLabel" = '' OR
                    "HeroPrimaryCtaUrl" = '' OR
                    "HeroSecondaryCtaLabel" = '' OR
                    "HeroSecondaryCtaUrl" = '' OR
                    "IntroTitle" = '' OR
                    "IntroDescription" = '' OR
                    "PlatformSectionLabel" = '' OR
                    "PlatformSectionTitle" = '' OR
                    "PlatformSectionDescription" = '' OR
                    "DirectionsText" = '' OR
                    "CountriesSectionLabel" = '' OR
                    "CountriesSectionTitle" = '' OR
                    "CountriesSectionDescription" = '' OR
                    "CountriesVisualTitle" = '' OR
                    "CountriesVisualDescription" = '' OR
                    "FeaturesSectionLabel" = '' OR
                    "FeaturesSectionTitle" = '' OR
                    "FeaturesSectionDescription" = '' OR
                    "FeatureCardsText" = '' OR
                    "LatestSectionLabel" = '' OR
                    "LatestSectionTitle" = '' OR
                    "LatestSectionDescription" = '' OR
                    "FeaturedPublicationEyebrow" = '' OR
                    "LatestFeaturedLinkLabel" = '' OR
                    "LatestIntelligenceLinkLabel" = '' OR
                    "AboutTitle" = '' OR
                    "AboutDescription" = '' OR
                    "CoverageAreasText" = '' OR
                    "FeaturedPublicationId" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM "HomePageContents"
                WHERE "Id" = '8a34e3b0-cab9-4a5a-a3f1-f7be2a336001'::uuid;
                """);
        }
    }
}
