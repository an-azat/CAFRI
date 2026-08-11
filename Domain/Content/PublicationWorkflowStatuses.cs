namespace CAFRI.Domain.Content;

public static class PublicationWorkflowStatuses
{
    public const string Draft = "Draft";
    public const string Published = "Published";
    public const string Rejected = "Rejected";
    public const string Archived = "Archived";

    public static readonly IReadOnlyList<string> All =
    [
        Draft,
        Published,
        Rejected,
        Archived
    ];

    public static bool IsValid(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        All.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase);

    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Draft;
        }

        var match = All.FirstOrDefault(item => string.Equals(item, value.Trim(), StringComparison.OrdinalIgnoreCase));
        return match ?? Draft;
    }

    public static bool IsPublishedStatus(string? value) =>
        string.Equals(Normalize(value), Published, StringComparison.OrdinalIgnoreCase);
}
