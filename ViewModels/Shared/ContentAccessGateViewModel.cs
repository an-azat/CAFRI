namespace CAFRI.ViewModels.Shared;

// Locked article/publication view shown to guests
// (documentation/template/cafri_article_signin_gate_demo.html).
public sealed class ContentAccessGateViewModel
{
    public IReadOnlyList<GateLinkViewModel> Breadcrumbs { get; init; } = [];

    public string Kicker { get; init; } = string.Empty;

    public required string Title { get; init; }

    public IReadOnlyList<string> MetaItems { get; init; } = [];

    public IReadOnlyList<string> Tags { get; init; } = [];

    public string? CoverImageUrl { get; init; }

    public string CoverLabel { get; init; } = string.Empty;

    public string CoverTitle { get; init; } = string.Empty;

    // Short teaser of the locked text; clamped to two lines in the view.
    public string Preview { get; init; } = string.Empty;

    public required string GateTitle { get; init; }

    public required string GateDescription { get; init; }

    public required string SignInLabel { get; init; }

    public required string SignInUrl { get; init; }

    public IReadOnlyList<GateDetailRowViewModel> DetailRows { get; init; } = [];

    public string RelatedTitle { get; init; } = "Related intelligence";

    public IReadOnlyList<GateRelatedItemViewModel> RelatedItems { get; init; } = [];
}

public sealed class GateLinkViewModel
{
    public required string Label { get; init; }

    public string? Url { get; init; }
}

public sealed class GateDetailRowViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }
}

public sealed class GateRelatedItemViewModel
{
    public required string Meta { get; init; }

    public required string Title { get; init; }

    public required string Url { get; init; }
}
