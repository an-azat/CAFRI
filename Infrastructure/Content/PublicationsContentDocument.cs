using CAFRI.ViewModels.Publications;

namespace CAFRI.Infrastructure.Content;

public sealed class PublicationsContentDocument
{
    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<string> TypeOptions { get; init; } = [];

    public IReadOnlyList<string> CountryOptions { get; init; } = [];

    public IReadOnlyList<string> TopicOptions { get; init; } = [];

    public IReadOnlyList<PublicationTabViewModel> Tabs { get; init; } = [];

    public IReadOnlyList<PublicationCardViewModel> Publications { get; init; } = [];

    public IReadOnlyList<PublicationCategoryItemViewModel> Categories { get; init; } = [];

    public IReadOnlyList<PublicationListItemViewModel> LatestPublications { get; init; } = [];

    public IReadOnlyList<PublicationBenefitViewModel> Benefits { get; init; } = [];

    public IReadOnlyList<PublicationDetailsViewModel> Details { get; init; } = [];
}
