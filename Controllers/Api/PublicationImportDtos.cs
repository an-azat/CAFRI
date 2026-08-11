using System.ComponentModel.DataAnnotations;

namespace CAFRI.Controllers.Api;

public sealed class PublicationImportDuplicateCheckRequest
{
    public string? ExternalId { get; init; }
    public string? SourceUrl { get; init; }
}

public sealed class PublicationImportDuplicateCheckResponse
{
    public bool IsDuplicate { get; init; }
    public string? MatchedBy { get; init; }
    public Guid? PublicationId { get; init; }
    public string? WorkflowStatus { get; init; }
    public string? Slug { get; init; }
}

public sealed class PublicationImportRequest
{
    public string? ExternalId { get; init; }

    [Required, StringLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, StringLength(2000)]
    public string Description { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string SourceUrl { get; init; } = string.Empty;

    [StringLength(220)]
    public string? SourceName { get; init; }

    [StringLength(1000)]
    public string? ImageUrl { get; init; }

    public DateTimeOffset? PublishedAtUtc { get; init; }

    public int? ReadingTimeMinutes { get; init; }

    [StringLength(160)]
    public string? AuthorLabel { get; init; }

    public IReadOnlyList<string>? Countries { get; init; }

    public IReadOnlyList<string>? Categories { get; init; }

    public bool RequiresReview { get; init; }
}

public sealed class PublicationImportResponse
{
    public Guid PublicationId { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string WorkflowStatus { get; init; } = string.Empty;
    public bool RequiresReview { get; init; }
    public IReadOnlyList<string> Countries { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
}
