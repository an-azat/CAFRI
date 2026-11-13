namespace CAFRI.Domain.Content;

/// <summary>
/// The four headline indicators every country's Economic snapshot shows, in this order. Labels
/// of editor KPIs and CountryIndicators rows are matched by keyword (e.g. "NBKR policy rate").
/// </summary>
public static class CountryProfileStandards
{
    public static readonly IReadOnlyList<SnapshotIndicator> SnapshotIndicators =
    [
        new("Real GDP growth", ["gdp growth"]),
        new("Annual inflation", ["inflation"]),
        new("Policy rate", ["policy rate", "base rate", "key rate", "refinancing rate"]),
        new("Public debt", ["public debt", "government debt", "sovereign debt"])
    ];

    public sealed record SnapshotIndicator(string Label, IReadOnlyList<string> Keywords)
    {
        public bool Matches(string? text) =>
            !string.IsNullOrWhiteSpace(text) &&
            Keywords.Any(keyword => text.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
