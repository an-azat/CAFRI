using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class IntelligenceContentEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(180)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string Category { get; init; } = string.Empty;

    [Required, StringLength(10)]
    public string CountryCode { get; init; } = string.Empty;

    [Required, StringLength(120)]
    public string CountryName { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string PublishedLabel { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required, StringLength(300)]
    public string Source { get; init; } = string.Empty;

    [StringLength(1000)]
    public string? HeroImageUrl { get; init; }

    public IFormFile? HeroImageFile { get; init; }

    public string? DetailsJson { get; init; }

    public string OverviewLead { get; init; } = string.Empty;

    public string OverviewBody { get; init; } = string.Empty;

    public string EffectiveDate { get; init; } = string.Empty;

    public string AppliesTo { get; init; } = string.Empty;

    public string DocumentTitle { get; init; } = string.Empty;

    public string DocumentSubtitle { get; init; } = string.Empty;

    public string DocumentMeta { get; init; } = string.Empty;

    public string DocumentDownloadUrl { get; init; } = string.Empty;

    public string KeyChangesText { get; init; } = string.Empty;

    public string ImpactAnalysisText { get; init; } = string.Empty;

    public string TimelineText { get; init; } = string.Empty;

    public string DocumentInformationText { get; init; } = string.Empty;

    public string KeyHighlightsText { get; init; } = string.Empty;

    public string RelatedOfficialSourcesText { get; init; } = string.Empty;

    public string RelatedIntelligenceText { get; init; } = string.Empty;

    public string MonitoringStatusText { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;
}
