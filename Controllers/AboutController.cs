using CAFRI.ViewModels.About;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class AboutController : Controller
{
    [HttpGet("/about")]
    public IActionResult Index()
    {
        var model = new AboutPageViewModel
        {
            Description = "CAFRI - Central Asia Financial & Regulatory Intelligence Initiative - is an analytical platform providing structured intelligence on financial regulation, banking sector developments, trade, sanctions, and macroeconomic trends across Central Asia.",
            DescriptionSecondary = "The platform helps institutions, regulators, businesses, researchers, and professionals monitor key developments, understand regional risks, and make informed decisions based on reliable and structured information.",
            AuthorDescription = "The project is developed by the author as an independent initiative focused on making Central Asia more transparent and accessible through structured financial, regulatory, and institutional intelligence. The goal is to combine regional expertise, verified sources, and a practical research framework in a single platform for professional use.",
            HeroFeatures =
            [
                new() { Icon = "independent", Title = "Independent", Description = "Non-commercial and impartial analytical perspective." },
                new() { Icon = "focused", Title = "Focused", Description = "Dedicated to Central Asia and adjacent Eurasian corridors." },
                new() { Icon = "reliable", Title = "Reliable", Description = "Based on verified official sources and structured methodology." },
                new() { Icon = "timely", Title = "Timely", Description = "Continuous monitoring of regulatory, financial, and economic updates." }
            ],
            MissionCard = new AboutSectionCardViewModel
            {
                Icon = "mission",
                Title = "Our Mission",
                Description = "To become a trusted source of financial and regulatory intelligence on Central Asia - supporting transparency, informed decision-making, and sustainable economic development in the region."
            },
            CoverageAreas =
            [
                new() { Icon = "institution", Title = "Financial Regulation", Description = "Central bank decisions, supervisory rules, licensing requirements, reporting obligations, and regulatory reforms." },
                new() { Icon = "bank", Title = "Banking Sector", Description = "Banking performance, monetary policy, capital requirements, liquidity, credit activity, and supervisory actions." },
                new() { Icon = "trade", Title = "Trade & Logistics", Description = "Trade corridors, customs procedures, cross-border transport, infrastructure projects, and regional connectivity." },
                new() { Icon = "shield", Title = "Sanctions & Compliance", Description = "Domestic and international sanctions, AML/CFT frameworks, FATF-related developments, and compliance obligations." },
                new() { Icon = "chart", Title = "Macroeconomics", Description = "GDP, inflation, foreign exchange reserves, fiscal policy, investment trends, and structural economic indicators." },
                new() { Icon = "government", Title = "Institutional Landscape", Description = "Government agencies, regulators, ministries, financial supervisors, statistics bodies, and international institutions." }
            ],
            GeographicCountries = ["Kazakhstan", "Kyrgyzstan", "Uzbekistan", "Tajikistan", "Turkmenistan"],
            CountryBadges = ["KZ", "KG", "UZ", "TJ", "TM"],
            Sources =
            [
                new() { Icon = "bank", Title = "Central Banks", Description = "Monetary policy, circulars, supervisory notices, and statistical releases." },
                new() { Icon = "institution", Title = "Financial Regulators", Description = "Licensing, prudential rules, enforcement actions, and reforms." },
                new() { Icon = "government", Title = "Finance Ministries", Description = "Fiscal policy, budget releases, debt strategy, and public finance updates." },
                new() { Icon = "shield", Title = "Supervisory Authorities", Description = "Sectoral guidance, compliance notices, and risk monitoring." },
                new() { Icon = "trade", Title = "Customs Administrations", Description = "Trade procedures, customs reforms, and corridor management updates." },
                new() { Icon = "chart", Title = "Statistics Agencies", Description = "Macroeconomic series, trade data, inflation, and industrial output." },
                new() { Icon = "globe", Title = "International Institutions", Description = "Regional programs, development finance, and multilateral assessments." },
                new() { Icon = "source", Title = "Other Official Sources", Description = "Presidential decrees, agency portals, and institutional statements." }
            ],
            WorkflowSteps =
            [
                new() { Number = 1, Title = "Collection", Description = "Gathering data and documents from official and verified sources." },
                new() { Number = 2, Title = "Verification", Description = "Cross-checking information and validating source reliability." },
                new() { Number = 3, Title = "Classification", Description = "Organizing information by country, sector, topic, and institution." },
                new() { Number = 4, Title = "Analysis", Description = "Transforming raw updates into structured insights and risk context." },
                new() { Number = 5, Title = "Publication", Description = "Publishing intelligence items, country profiles, reports, and alerts." },
                new() { Number = 6, Title = "Updates", Description = "Monitoring changes and keeping information up to date." }
            ],
            Audiences =
            [
                new() { Icon = "bank", Title = "Banks & Financial Institutions" },
                new() { Icon = "shield", Title = "Regulators & Supervisors" },
                new() { Icon = "government", Title = "Government Agencies" },
                new() { Icon = "chart", Title = "Investors & Corporates" },
                new() { Icon = "brief", Title = "Consulting & Law Firms" },
                new() { Icon = "research", Title = "Researchers & Analysts" },
                new() { Icon = "globe", Title = "International Organizations" }
            ],
            WhyItMattersPoints =
            [
                "Comprehensive regional coverage",
                "Focus on financial and regulatory topics",
                "Source-based and evidence-driven analysis",
                "Clear structure by country and sector",
                "Timely alerts and regular updates",
                "User-friendly platform for professionals"
            ]
        };

        return View(model);
    }
}
