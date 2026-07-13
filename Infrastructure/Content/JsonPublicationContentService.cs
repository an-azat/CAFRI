using CAFRI.Application.Abstractions.Services;
using CAFRI.ViewModels.Publications;

namespace CAFRI.Infrastructure.Content;

public sealed class JsonPublicationContentService : IPublicationContentService
{
    private readonly Lazy<PublicationsContentDocument> _document;

    public JsonPublicationContentService(JsonContentFileLoader loader)
    {
        _document = new Lazy<PublicationsContentDocument>(() => loader.Load<PublicationsContentDocument>("App_Data/content/publications.json"));
    }

    public PublicationsPageViewModel GetIndexPage(
        string? query = null,
        string? type = null,
        string? country = null,
        string? topic = null,
        string? tab = null,
        int page = 1)
    {
        var document = _document.Value;
        return new PublicationsPageViewModel
        {
            Query = query?.Trim() ?? string.Empty,
            SelectedType = string.IsNullOrWhiteSpace(type) ? "All Types" : type.Trim(),
            SelectedCountry = string.IsNullOrWhiteSpace(country) ? "All Countries" : country.Trim(),
            SelectedTopic = string.IsNullOrWhiteSpace(topic) ? "All Topics" : topic.Trim(),
            SelectedTab = string.IsNullOrWhiteSpace(tab) ? "latest" : tab.Trim().ToLowerInvariant(),
            CurrentPage = 1,
            TotalPages = 1,
            TotalResults = document.Publications.Count,
            ShowingFrom = document.Publications.Count == 0 ? 0 : 1,
            ShowingTo = document.Publications.Count,
            Description = document.Description,
            TypeOptions = document.TypeOptions,
            CountryOptions = document.CountryOptions,
            TopicOptions = document.TopicOptions,
            Tabs = document.Tabs,
            Publications = document.Publications,
            Categories = document.Categories,
            LatestPublications = document.LatestPublications,
            Benefits = document.Benefits
        };
    }

    public PublicationDetailsViewModel? GetDetails(string slug)
    {
        var document = _document.Value;
        var explicitDetail = document.Details.FirstOrDefault(item =>
            string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        if (explicitDetail is not null)
        {
            explicitDetail.CanAccessFullContent = false;
            return explicitDetail;
        }

        var card = document.Publications.FirstOrDefault(item =>
            string.Equals(item.Slug, slug, StringComparison.OrdinalIgnoreCase));

        return card is null ? null : BuildFallbackDetails(card, document.Publications);
    }

    private static PublicationDetailsViewModel BuildFallbackDetails(
        PublicationCardViewModel card,
        IReadOnlyList<PublicationCardViewModel> publications)
    {
        var relatedCards = publications
            .Where(item => !string.Equals(item.Slug, card.Slug, StringComparison.OrdinalIgnoreCase))
            .Take(4)
            .ToArray();

        return new PublicationDetailsViewModel
        {
            Title = card.Title,
            Slug = card.Slug,
            PublicationType = card.Type,
            Topic = card.Type,
            CoverageLabel = card.CountryLabel,
            ShortDescription = card.Description,
            PublishedDate = card.Meta.Split('|')[0].Trim(),
            ReadingTime = card.Meta.Contains('|', StringComparison.Ordinal) ? card.Meta.Split('|')[1].Trim() : "12 min read",
            AuthorLabel = "CAFRI Research Team",
            DocumentLabel = "PDF available | 1.2 MB",
            HeroVisualClassName = "publication-hero-media--trade-corridors",
            PdfDownloadUrl = "#",
            IsPremium = true,
            Actions =
            [
                new() { Label = "Read Online" },
                new() { Label = "Download Full Report (PDF)" },
                new() { Label = "Share" },
                new() { Label = "Save" },
                new() { Label = "Cite" },
                new() { Label = "Print" }
            ],
            Tabs =
            [
                new() { Label = "Overview", TargetId = "overview", IsActive = true },
                new() { Label = "Key Findings", TargetId = "key-findings" },
                new() { Label = "Data & Charts", TargetId = "data-charts" },
                new() { Label = "Documents", TargetId = "related-documents" },
                new() { Label = "Related Publications", TargetId = "related-publications" }
            ],
            Kpis =
            [
                new() { Label = "Coverage", Value = card.CountryLabel, Detail = "Primary focus", Tone = "accent" },
                new() { Label = "Publication Type", Value = card.Type, Detail = "Research format", Tone = "neutral" },
                new() { Label = "Reading Time", Value = card.Meta.Contains('|', StringComparison.Ordinal) ? card.Meta.Split('|')[1].Trim() : "12 min", Detail = "Estimated duration", Tone = "neutral" },
                new() { Label = "Update Cycle", Value = "Monthly", Detail = "Monitoring cadence", Tone = "positive" },
                new() { Label = "Access Level", Value = "Public", Detail = "Web article available", Tone = "accent" }
            ],
            ExecutiveSummaryParagraphs =
            [
                $"{card.Title} provides a structured analytical overview of current developments relevant to {card.CountryLabel}. The page is designed as an online publication rather than a simple download screen, with summary findings, embedded data blocks, and related materials presented directly on the platform.",
                "This fallback detail layout keeps the same CAFRI analytical structure so the publication can be reviewed on-site, while remaining ready for future replacement with fully custom report content."
            ],
            ExecutiveHighlights =
            [
                "Core summary available directly on the page",
                "Structured sections ready for expanded editorial content",
                "Data, documents, and related materials grouped in one analytical workflow"
            ],
            KeyFindings =
            [
                new() { Icon = "01", Title = "Structured publication format", Description = "The material is displayed as a navigable article page with sections, data, and supporting documents." },
                new() { Icon = "02", Title = "Country and topic context", Description = $"The current report is positioned within CAFRI coverage for {card.CountryLabel} and related thematic monitoring." },
                new() { Icon = "03", Title = "Expandable content model", Description = "The current structure is ready for richer report-specific charts, maps, and narrative updates as content grows." }
            ],
            Sections =
            [
                new()
                {
                    Id = "regional-context",
                    Heading = "Regional Context",
                    Paragraphs =
                    [
                        $"This publication sits within CAFRI's broader analytical coverage of {card.CountryLabel} and Central Asian financial and regulatory developments.",
                        "The article template supports incremental enrichment with report-specific findings, source references, and visual evidence without changing the overall page architecture."
                    ]
                },
                new()
                {
                    Id = "policy-reforms",
                    Heading = "Analytical Context",
                    Paragraphs =
                    [
                        "Each publication detail page is intended to hold a durable analytical narrative rather than only a downloadable document.",
                        "That means users can access key findings, supporting data, and connected materials directly in the interface before deciding whether to download a PDF version."
                    ]
                }
            ],
            Charts =
            [
                new()
                {
                    Title = "Monitoring Snapshot",
                    Subtitle = "Illustrative publication trend view",
                    ChartType = "line",
                    Labels = ["Q1", "Q2", "Q3", "Q4"],
                    Series =
                    [
                        new() { Label = "Signal Intensity", Color = "#1476ff", Values = [52m, 58m, 61m, 67m] }
                    ]
                },
                new()
                {
                    Title = "Topic Allocation",
                    Subtitle = "Illustrative thematic distribution",
                    ChartType = "doughnut",
                    ValueSuffix = "%",
                    Labels = ["Policy", "Market", "Institutions", "Risk"],
                    Series =
                    [
                        new() { Label = "Share", Color = "#1476ff", Values = [30m, 24m, 21m, 25m] }
                    ]
                }
            ],
            TableRows =
            [
                new() { Corridor = "Primary Focus", CountriesCovered = card.CountryLabel, MainRoute = "Publication-specific", Volume = "-", Growth = "-", Status = "Active" },
                new() { Corridor = "Regional Comparison", CountriesCovered = "Central Asia", MainRoute = "Comparative view", Volume = "-", Growth = "-", Status = "Monitored" }
            ],
            MapRoutes =
            [
                new() { Name = "Coverage Lens", Countries = card.CountryLabel, TransitVolume = "N/A", AverageTime = "N/A", Status = "Monitoring", AccentColor = "#1476ff" }
            ],
            GalleryItems =
            [
                new() { Title = "Institutional view", Caption = "Visual placeholder for publication-specific photography or field imagery.", VisualClassName = "publication-gallery__visual--port" },
                new() { Title = "Analytical dashboard", Caption = "Visual placeholder for charts, dashboards, or scanned source documents.", VisualClassName = "publication-gallery__visual--dry-port" },
                new() { Title = "Regional infrastructure", Caption = "Visual placeholder for maps, facilities, or sector-specific assets.", VisualClassName = "publication-gallery__visual--rail" },
                new() { Title = "Source materials", Caption = "Visual placeholder for annexes, filings, and supporting visual sources.", VisualClassName = "publication-gallery__visual--customs" }
            ],
            Documents =
            [
                new() { Title = $"{card.Title} - Full PDF", Type = card.Type, Meta = $"{card.Meta} | PDF", DownloadUrl = "#" }
            ],
            TableOfContents =
            [
                new() { Label = "1. Executive Summary", TargetId = "overview" },
                new() { Label = "2. Regional Context", TargetId = "regional-context" },
                new() { Label = "3. Analytical Context", TargetId = "policy-reforms" },
                new() { Label = "4. Data & Charts", TargetId = "data-charts" }
            ],
            KeyTopics =
            [
                new() { Label = card.Type, TargetId = "key-findings" },
                new() { Label = card.CountryLabel, TargetId = "regional-context" }
            ],
            SidebarDocuments =
            [
                new() { Title = $"{card.Title} - Full PDF", Meta = $"{card.Meta} | PDF" }
            ],
            RelatedIntelligence =
            [
                new() { Title = $"{card.CountryLabel} monitoring overview", Meta = "Related intelligence material", Url = "/intelligence" }
            ],
            PublicationInfo =
            [
                new() { Label = "Publication Type", Value = card.Type },
                new() { Label = "Coverage", Value = card.CountryLabel },
                new() { Label = "Published", Value = card.Meta.Split('|')[0].Trim() },
                new() { Label = "Author", Value = "CAFRI Research Team" },
                new() { Label = "Access", Value = "Public" }
            ],
            RelatedPublications = relatedCards
        };
    }
}
