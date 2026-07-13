using CAFRI.Application.Abstractions.Services;
using CAFRI.ViewModels.Intelligence;

namespace CAFRI.Infrastructure.Content;

public sealed class JsonIntelligenceContentService : IIntelligenceContentService
{
    private readonly Lazy<IntelligenceContentDocument> _document;

    public JsonIntelligenceContentService(JsonContentFileLoader loader)
    {
        _document = new Lazy<IntelligenceContentDocument>(() => loader.Load<IntelligenceContentDocument>("App_Data/content/intelligence.json"));
    }

    public IntelligencePageViewModel GetIndexPage(
        string? search = null,
        string? country = null,
        string? category = null,
        string? institution = null,
        string? dateRange = null,
        string? sortBy = null,
        int page = 1)
    {
        var document = _document.Value;
        return new IntelligencePageViewModel
        {
            Description = document.Description,
            SearchPlaceholder = document.SearchPlaceholder,
            CountrySelectOptions = document.CountrySelectOptions,
            CategorySelectOptions = document.CategorySelectOptions,
            InstitutionSelectOptions = document.InstitutionSelectOptions,
            SidebarCountries = document.SidebarCountries,
            SidebarCategories = document.SidebarCategories,
            DateRangeOptions = document.DateRangeOptions,
            Items = document.Items,
            SummaryMetrics =
            [
                new() { Value = document.TotalResults.ToString(), Label = "Published Items" },
                new() { Value = document.Items.Select(x => x.CountryName).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Label = "Countries Covered" },
                new() { Value = document.Items.Select(x => x.Category).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Label = "Core Categories" },
                new() { Value = document.Items.Select(x => x.Source).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Label = "Sources Referenced" }
            ],
            SearchTerm = search,
            CountryFilter = string.IsNullOrWhiteSpace(country) ? "All Countries" : country,
            CategoryFilter = string.IsNullOrWhiteSpace(category) ? "All Categories" : category,
            InstitutionFilter = string.IsNullOrWhiteSpace(institution) ? "All Institutions" : institution,
            DateRangeFilter = string.IsNullOrWhiteSpace(dateRange) ? "Any time" : dateRange,
            ShowingFrom = document.ShowingFrom,
            ShowingTo = document.ShowingTo,
            TotalResults = document.TotalResults,
            CurrentPage = page <= 0 ? document.CurrentPage : page,
            TotalPages = document.TotalPages,
            PageSize = document.Items.Count,
            SortBy = string.IsNullOrWhiteSpace(sortBy) ? document.SortBy : sortBy,
            ProfessionalAccessBenefits = document.ProfessionalAccessBenefits
        };
    }

    public IntelligenceDetailsViewModel? GetDetails(string slug)
    {
        var document = _document.Value;
        var explicitDetail = document.Details.FirstOrDefault(item =>
            string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        if (explicitDetail is not null)
        {
            return explicitDetail;
        }

        var item = document.Items.FirstOrDefault(entry =>
            string.Equals(entry.Slug, slug, StringComparison.OrdinalIgnoreCase));

        return item is null ? null : BuildFallbackDetails(item);
    }

    private static IntelligenceDetailsViewModel BuildFallbackDetails(IntelligenceItemCardViewModel item) =>
        new()
        {
            Title = item.Title,
            Slug = item.Slug,
            Category = item.Category,
            CountryCode = item.CountryCode,
            CountryName = item.CountryName,
            ShortDescription = item.Description,
            HeroImageClassName = item.CountryCode switch
            {
                "UZ" => "article-hero-image--uzbekistan",
                "KG" => "article-hero-image--kyrgyzstan",
                "TJ" => "article-hero-image--tajikistan",
                "TM" => "article-hero-image--turkmenistan",
                _ => "article-hero-image--kazakhstan"
            },
            PublishedDate = item.PublishedLabel,
            ReadingTime = "8 min read",
            AnalysisType = "CAFRI Intelligence Note",
            OverviewLead = $"{item.Title} is currently available as a structured intelligence note within the CAFRI platform.",
            OverviewBody = "This fallback detail page preserves the analytical layout and gives the project a consistent data-driven path while richer material is still being loaded into the content layer.",
            EffectiveDate = "Monitoring",
            AppliesTo = "Relevant public and private sector stakeholders",
            IsPremium = true,
            Actions =
            [
                new() { Label = "Save", Icon = "save" },
                new() { Label = "Cite", Icon = "cite" },
                new() { Label = "Share", Icon = "share" },
                new() { Label = "Print", Icon = "print" }
            ],
            Tabs =
            [
                new() { Label = "Overview", IsActive = true },
                new() { Label = "Key Changes" },
                new() { Label = "Impact Analysis" },
                new() { Label = "Timeline" },
                new() { Label = "Regulatory Text" },
                new() { Label = "Data & Charts" },
                new() { Label = "Related Materials" }
            ],
            KeyChanges =
            [
                new() { Number = 1, Title = "Structured Detail View", Description = "The material is now exposed through the shared intelligence detail template instead of controller hardcode." },
                new() { Number = 2, Title = "Content-First Delivery", Description = "Editorial content can be expanded directly in the JSON content layer without changing controller logic." },
                new() { Number = 3, Title = "Reusable Monitoring Model", Description = "The same schema can support regulatory, banking, trade, sanctions, and macroeconomic items." }
            ],
            ImpactAnalysis =
            [
                new() { Title = "Institutions", Description = "Monitoring teams can add and update content without touching routing and page composition.", ImpactLabel = "Impact: Positive", ImpactTone = "impact-positive" },
                new() { Title = "Platform", Description = "The project now has a real data-driven seam between presentation and content.", ImpactLabel = "Impact: High", ImpactTone = "impact-high" },
                new() { Title = "Users", Description = "Users get a stable detail-page experience even before every item receives a custom deep-dive.", ImpactLabel = "Impact: Medium", ImpactTone = "impact-medium" }
            ],
            TimelineItems =
            [
                new() { DateLabel = item.PublishedLabel, Description = "Intelligence item published", Stage = "past" },
                new() { DateLabel = "Current", Description = "Content monitored in CAFRI", Stage = "current" },
                new() { DateLabel = "Next update", Description = "Detail content can be expanded from the content layer", Stage = "future" }
            ],
            OfficialDocument = new IntelligenceOfficialDocumentViewModel
            {
                Title = "Official document or source reference",
                Subtitle = item.Source,
                Meta = $"{item.PublishedLabel} | Source-based item"
            },
            DocumentInformation =
            [
                new() { Label = "Category", Value = item.Category },
                new() { Label = "Country", Value = item.CountryName },
                new() { Label = "Published", Value = item.PublishedLabel },
                new() { Label = "Source", Value = item.Source },
                new() { Label = "Status", Value = "Monitoring" }
            ],
            KeyHighlights =
            [
                "Data-driven content source in place",
                "Fallback detail view available for unpublished deep dives",
                "Ready for progressive enrichment by editors"
            ],
            RelatedOfficialSources =
            [
                new() { Title = item.Source, Subtitle = "Primary source reference" }
            ],
            RelatedIntelligence =
            [
                new() { Title = "More intelligence items", Subtitle = "Browse the full catalog", Url = "/intelligence" }
            ],
            MonitoringStatus =
            [
                new() { Label = "Status", Value = "Monitoring", IsStatus = true },
                new() { Label = "Source", Value = item.Source },
                new() { Label = "Coverage", Value = item.CountryName }
            ]
        };
}
