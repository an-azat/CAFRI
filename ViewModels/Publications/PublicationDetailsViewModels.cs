namespace CAFRI.ViewModels.Publications;

using CAFRI.ViewModels.Shared;

public sealed class PublicationDetailsViewModel
{
    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string PublicationType { get; init; }

    public required string Topic { get; init; }

    public required string CoverageLabel { get; init; }

    public required string ShortDescription { get; init; }

    public required string PublishedDate { get; init; }

    public required string ReadingTime { get; init; }

    public required string AuthorLabel { get; init; }

    public required string DocumentLabel { get; init; }

    public required string HeroVisualClassName { get; init; }

    public string HeroImageUrl { get; set; } = string.Empty;

    public string PdfDownloadUrl { get; init; } = "#";

    public string ExecutiveSummaryHtml { get; init; } = string.Empty;

    public bool IsPremium { get; init; }

    public bool CanAccessFullContent { get; set; }

    public IReadOnlyList<PublicationActionItemViewModel> Actions { get; init; } = [];

    public IReadOnlyList<PublicationDetailsTabViewModel> Tabs { get; init; } = [];

    public IReadOnlyList<PublicationKpiCardViewModel> Kpis { get; init; } = [];

    public IReadOnlyList<string> ExecutiveSummaryParagraphs { get; init; } = [];

    public IReadOnlyList<string> ExecutiveHighlights { get; init; } = [];

    public IReadOnlyList<PublicationFindingCardViewModel> KeyFindings { get; init; } = [];

    public IReadOnlyList<PublicationArticleSectionViewModel> Sections { get; init; } = [];

    public IReadOnlyList<PublicationChartViewModel> Charts { get; init; } = [];

    public IReadOnlyList<PublicationTableRowViewModel> TableRows { get; init; } = [];

    public IReadOnlyList<PublicationMapRouteViewModel> MapRoutes { get; init; } = [];

    public IReadOnlyList<PublicationGalleryItemViewModel> GalleryItems { get; init; } = [];

    public IReadOnlyList<PublicationDocumentCardViewModel> Documents { get; init; } = [];

    public IReadOnlyList<PublicationSidebarSectionLinkViewModel> TableOfContents { get; init; } = [];

    public IReadOnlyList<PublicationSidebarSectionLinkViewModel> KeyTopics { get; init; } = [];

    public IReadOnlyList<PublicationSidebarLinkItemViewModel> SidebarDocuments { get; init; } = [];

    public IReadOnlyList<PublicationSidebarLinkItemViewModel> RelatedIntelligence { get; init; } = [];

    public IReadOnlyList<PublicationInfoRowViewModel> PublicationInfo { get; init; } = [];

    public IReadOnlyList<PublicationCardViewModel> RelatedPublications { get; init; } = [];
}

public sealed class PublicationActionItemViewModel
{
    public required string Label { get; init; }
}

public sealed class PublicationDetailsTabViewModel
{
    public required string Label { get; init; }

    public required string TargetId { get; init; }

    public bool IsActive { get; init; }
}

public sealed class PublicationKpiCardViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public required string Detail { get; init; }

    public string Tone { get; init; } = "neutral";
}

public sealed class PublicationFindingCardViewModel
{
    public required string Icon { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }
}

public sealed class PublicationArticleSectionViewModel
{
    public required string Id { get; init; }

    public required string Heading { get; init; }

    public IReadOnlyList<string> Paragraphs { get; init; } = [];

    public IReadOnlyList<string> BulletPoints { get; init; } = [];

    public PublicationCalloutViewModel? Callout { get; init; }
}

public sealed class PublicationCalloutViewModel
{
    public required string Tone { get; init; }

    public required string Label { get; init; }

    public required string Title { get; init; }

    public required string Description { get; init; }
}

public sealed class PublicationChartViewModel
{
    public required string Title { get; init; }

    public required string Subtitle { get; init; }

    public required string ChartType { get; init; }

    public string ValueSuffix { get; init; } = string.Empty;

    public IReadOnlyList<string> Labels { get; init; } = [];

    public IReadOnlyList<PublicationChartSeriesViewModel> Series { get; init; } = [];
}

public sealed class PublicationChartSeriesViewModel
{
    public required string Label { get; init; }

    public required string Color { get; init; }

    public IReadOnlyList<decimal> Values { get; init; } = [];
}

public sealed class PublicationTableRowViewModel
{
    public required string Corridor { get; init; }

    public required string CountriesCovered { get; init; }

    public required string MainRoute { get; init; }

    public required string Volume { get; init; }

    public required string Growth { get; init; }

    public required string Status { get; init; }
}

public sealed class PublicationMapRouteViewModel
{
    public required string Name { get; init; }

    public required string Countries { get; init; }

    public required string TransitVolume { get; init; }

    public required string AverageTime { get; init; }

    public required string Status { get; init; }

    public required string AccentColor { get; init; }
}

public sealed class PublicationGalleryItemViewModel
{
    public string ImageUrl { get; set; } = string.Empty;

    public string Alt { get; set; } = string.Empty;

    public required string Title { get; init; }

    public required string Caption { get; init; }

    public string VisualClassName { get; init; } = string.Empty;
}

public sealed class PublicationDocumentCardViewModel
{
    public required string Title { get; init; }

    public required string Type { get; init; }

    public required string Meta { get; init; }

    public string DownloadUrl { get; init; } = "#";

    public bool IsDownloadable => !string.IsNullOrWhiteSpace(DownloadUrl) && DownloadUrl != "#";
}

public sealed class PublicationSidebarSectionLinkViewModel
{
    public required string Label { get; init; }

    public required string TargetId { get; init; }
}

public sealed class PublicationSidebarLinkItemViewModel
{
    public required string Title { get; init; }

    public required string Meta { get; init; }

    public string Url { get; init; } = "#";
}

public sealed class PublicationInfoRowViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }
}
