namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceDetailsViewModel
{
    public required string Title { get; init; }

    public required string Slug { get; init; }

    public required string Category { get; init; }

    public required string CountryCode { get; init; }

    public required string CountryName { get; init; }

    public required string ShortDescription { get; init; }

    public required string HeroImageClassName { get; init; }

    public string HeroImageUrl { get; set; } = string.Empty;

    public required string PublishedDate { get; init; }

    public required string ReadingTime { get; init; }

    public required string AnalysisType { get; init; }

    public required string OverviewLead { get; init; }

    public required string OverviewBody { get; init; }

    public required string EffectiveDate { get; init; }

    public required string AppliesTo { get; init; }

    public bool IsPremium { get; init; }

    public bool CanAccessFullContent { get; set; }

    public IReadOnlyList<IntelligenceActionItemViewModel> Actions { get; init; } = [];

    public IReadOnlyList<IntelligenceTabViewModel> Tabs { get; init; } = [];

    public IReadOnlyList<IntelligenceKeyChangeViewModel> KeyChanges { get; init; } = [];

    public IReadOnlyList<IntelligenceImpactCardViewModel> ImpactAnalysis { get; init; } = [];

    public IReadOnlyList<IntelligenceTimelineItemViewModel> TimelineItems { get; init; } = [];

    public required IntelligenceOfficialDocumentViewModel OfficialDocument { get; init; }

    public IReadOnlyList<IntelligenceDocumentInfoRowViewModel> DocumentInformation { get; init; } = [];

    public IReadOnlyList<string> KeyHighlights { get; init; } = [];

    public IReadOnlyList<IntelligenceSidebarLinkViewModel> RelatedOfficialSources { get; init; } = [];

    public IReadOnlyList<IntelligenceSidebarLinkViewModel> RelatedIntelligence { get; init; } = [];

    public IReadOnlyList<IntelligenceStatusRowViewModel> MonitoringStatus { get; init; } = [];
}
