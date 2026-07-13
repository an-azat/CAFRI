using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class PublicationContentEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(180)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string Type { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableTypes { get; init; } = [];

    [Required, StringLength(10)]
    public string CountryCode { get; init; } = string.Empty;

    public IReadOnlyList<AdminCountryOptionViewModel> AvailableCountries { get; init; } = [];

    [Required, StringLength(120)]
    public string CountryLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required, StringLength(200)]
    public string Meta { get; init; } = string.Empty;

    public string? DetailsJson { get; init; }

    [StringLength(120)]
    public string Topic { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableTopics { get; init; } = [];

    [StringLength(120)]
    public string PublishedDate { get; init; } = string.Empty;

    public string PublishedMonth { get; init; } = string.Empty;

    [StringLength(120)]
    public string ReadingTime { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableReadingTimes { get; init; } = [];

    [StringLength(160)]
    public string AuthorLabel { get; init; } = string.Empty;

    [StringLength(160)]
    public string DocumentLabel { get; init; } = string.Empty;

    [StringLength(120)]
    public string HeroVisualClassName { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? HeroImageUrl { get; init; }

    public IFormFile? HeroImageFile { get; init; }

    public string? GalleryText { get; init; }

    public IFormFile[]? GalleryFiles { get; init; }

    [StringLength(500)]
    public string PdfDownloadUrl { get; init; } = string.Empty;

    public string ActionsText { get; init; } = string.Empty;

    public string ExecutiveSummaryText { get; init; } = string.Empty;

    public string ExecutiveHighlightsText { get; init; } = string.Empty;

    public string KeyFindingsText { get; init; } = string.Empty;

    public string SectionsText { get; init; } = string.Empty;

    public string DocumentsText { get; init; } = string.Empty;

    public string RelatedIntelligenceText { get; init; } = string.Empty;

    public string PublicationInfoText { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;
}
