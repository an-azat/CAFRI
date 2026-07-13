namespace CAFRI.ViewModels.Shared;

public sealed class ProfessionalAccessCtaViewModel
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string ButtonText { get; init; }

    public string ButtonUrl { get; init; } = "/access/request";

    public IReadOnlyList<string> Benefits { get; init; } = [];
}
