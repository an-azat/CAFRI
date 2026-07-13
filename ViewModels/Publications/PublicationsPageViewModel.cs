namespace CAFRI.ViewModels.Publications;

public sealed class PublicationsPageViewModel
{
    public string Query { get; init; } = string.Empty;

    public string SelectedType { get; init; } = "All Types";

    public string SelectedCountry { get; init; } = "All Countries";

    public string SelectedTopic { get; init; } = "All Topics";

    public string SelectedTab { get; init; } = "latest";

    public int CurrentPage { get; init; } = 1;

    public int TotalPages { get; init; } = 1;

    public int TotalResults { get; init; }

    public int ShowingFrom { get; init; }

    public int ShowingTo { get; init; }

    public string Title { get; init; } = "Publications";

    public string Description { get; init; } = string.Empty;

    public IReadOnlyList<string> TypeOptions { get; init; } = [];

    public IReadOnlyList<string> CountryOptions { get; init; } = [];

    public IReadOnlyList<string> TopicOptions { get; init; } = [];

    public IReadOnlyList<PublicationTabViewModel> Tabs { get; init; } = [];

    public IReadOnlyList<PublicationCardViewModel> Publications { get; init; } = [];

    public IReadOnlyList<PublicationCategoryItemViewModel> Categories { get; init; } = [];

    public IReadOnlyList<PublicationListItemViewModel> LatestPublications { get; init; } = [];

    public IReadOnlyList<PublicationBenefitViewModel> Benefits { get; init; } = [];
}
