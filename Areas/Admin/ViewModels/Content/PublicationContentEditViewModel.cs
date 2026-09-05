using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class PublicationContentEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(180)]
    [Display(Name = "URL Slug")]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(40)]
    [Display(Name = "Status")]
    public string WorkflowStatus { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableStatuses { get; init; } = [];

    [Display(Name = "Requires Editorial Review")]
    public bool RequiresReview { get; init; }

    [StringLength(300)]
    [Display(Name = "External ID")]
    public string? ExternalId { get; init; }

    [StringLength(220)]
    [Display(Name = "Source Name")]
    public string? SourceName { get; init; }

    [StringLength(1000)]
    [Display(Name = "Source URL")]
    public string? SourceUrl { get; init; }

    [StringLength(220)]
    [Display(Name = "Source Domain")]
    public string? SourceDomain { get; init; }

    [Display(Name = "Original Publish Date/Time")]
    public string? SourcePublishedAtUtc { get; init; }

    [Required, StringLength(120)]
    [Display(Name = "Publication Type")]
    public string Type { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableTypes { get; init; } = [];

    [Required, StringLength(10)]
    [Display(Name = "Country")]
    public string CountryCode { get; init; } = string.Empty;

    public IReadOnlyList<AdminCountryOptionViewModel> AvailableCountries { get; init; } = [];

    [Required, StringLength(120)]
    [Display(Name = "Country Name")]
    public string CountryLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    [Display(Name = "Title")]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    [Display(Name = "Short Description")]
    public string Description { get; init; } = string.Empty;

    [Required, StringLength(200)]
    [Display(Name = "Meta Summary")]
    public string Meta { get; init; } = string.Empty;

    public string? DetailsJson { get; init; }

    [StringLength(120)]
    [Display(Name = "Topic")]
    public string Topic { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableTopics { get; init; } = [];

    [StringLength(120)]
    [Display(Name = "Published Date")]
    public string? PublishedDate { get; init; } = string.Empty;

    [Display(Name = "Published Month")]
    public string? PublishedMonth { get; init; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Reading Time")]
    public string ReadingTime { get; init; } = string.Empty;

    public IReadOnlyList<string> AvailableReadingTimes { get; init; } = [];

    [StringLength(160)]
    [Display(Name = "Author")]
    public string AuthorLabel { get; init; } = string.Empty;

    [StringLength(160)]
    [Display(Name = "Document Label")]
    public string? DocumentLabel { get; init; } = string.Empty;

    [StringLength(120)]
    [Display(Name = "Hero Visual Style")]
    public string? HeroVisualClassName { get; init; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Hero Image URL")]
    public string? HeroImageUrl { get; init; }

    public IFormFile? HeroImageFile { get; init; }

    [Display(Name = "Photo Gallery")]
    public string? GalleryText { get; init; }

    public IFormFile[]? GalleryFiles { get; init; }

    public IFormFile[]? DocumentFiles { get; init; }

    [StringLength(500)]
    [Display(Name = "Legacy PDF Download URL")]
    public string? PdfDownloadUrl { get; init; } = string.Empty;

    [Display(Name = "Assigned Countries")]
    public string? AssignedCountriesText { get; init; } = string.Empty;

    [Display(Name = "Assigned Categories")]
    public string? AssignedCategoriesText { get; init; } = string.Empty;

    [Display(Name = "Action Buttons")]
    public string? ActionsText { get; init; } = string.Empty;

    [Display(Name = "Executive Summary")]
    public string? ExecutiveSummaryText { get; init; } = string.Empty;

    [Display(Name = "Executive Highlights")]
    public string? ExecutiveHighlightsText { get; init; } = string.Empty;

    [Display(Name = "Key Findings")]
    public string? KeyFindingsText { get; init; } = string.Empty;

    [Display(Name = "Analytical Sections")]
    public string? SectionsText { get; init; } = string.Empty;

    [Display(Name = "Charts")]
    public string? ChartsText { get; init; }

    [Display(Name = "Transit Data Table Rows")]
    public string? TableRowsText { get; init; }

    [Display(Name = "Document Library")]
    public string? DocumentsText { get; init; } = string.Empty;

    [Display(Name = "Document Upload Mapping")]
    public string? DocumentUploadMapText { get; init; }

    [Display(Name = "Related Intelligence Links")]
    public string? RelatedIntelligenceText { get; init; } = string.Empty;

    [Display(Name = "Publication Info Rows")]
    public string? PublicationInfoText { get; init; } = string.Empty;

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; init; }

    [Display(Name = "Published")]
    public bool IsPublished { get; init; } = true;
}
