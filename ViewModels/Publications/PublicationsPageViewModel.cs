namespace CAFRI.ViewModels.Publications;

public sealed class PublicationsPageViewModel
{
    public string Query { get; init; } = string.Empty;

    public string SelectedType { get; init; } = "All Types";

    public string SelectedCountry { get; init; } = "All Countries";

    public string SelectedTopic { get; init; } = "All Topics";

    public string SortBy { get; init; } = "Newest";

    public int CurrentPage { get; init; } = 1;

    public int TotalPages { get; init; } = 1;

    public int TotalResults { get; init; }

    public int ShowingTo { get; init; }

    public IReadOnlyList<string> TypeOptions { get; init; } = [];

    public IReadOnlyList<string> CountryOptions { get; init; } = [];

    public IReadOnlyList<string> TopicOptions { get; init; } = [];

    public IReadOnlyList<PublicationCardViewModel> Publications { get; init; } = [];

    public bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(Query) ||
        SelectedType != "All Types" ||
        SelectedCountry != "All Countries" ||
        SelectedTopic != "All Topics" ||
        SortBy != "Newest";
}
