namespace CAFRI.Infrastructure.Content;

public static class PublicationTopicMapper
{
    public static readonly string[] CanonicalTypes =
    [
        "REPORT",
        "COUNTRY PROFILE",
        "ANALYTICAL BRIEF",
        "REGULATORY NOTE",
        "BANKING REVIEW",
        "SANCTIONS ANALYSIS",
        "RESEARCH PAPER"
    ];

    public static readonly string[] CanonicalTopics =
    [
        "Banking",
        "Regulation",
        "Trade & Logistics",
        "Sanctions",
        "Macroeconomics",
        "Energy Finance"
    ];

    public static string Normalize(string? topic, string? type, string? title, string? description)
    {
        var explicitTopic = NormalizeExplicitTopic(topic);
        if (!string.IsNullOrWhiteSpace(explicitTopic))
        {
            return explicitTopic;
        }

        var signal = string.Join(
            " ",
            new[]
            {
                type ?? string.Empty,
                title ?? string.Empty,
                description ?? string.Empty
            }).ToLowerInvariant();

        if (ContainsAny(signal, "sanction", "aml", "cft", "compliance"))
        {
            return "Sanctions";
        }

        if (ContainsAny(signal, "trade", "logistics", "corridor", "transit", "port", "rail", "customs", "route"))
        {
            return "Trade & Logistics";
        }

        if (ContainsAny(signal, "inflation", "gdp", "fiscal", "monetary", "macro", "policy rate"))
        {
            return "Macroeconomics";
        }

        if (ContainsAny(signal, "energy"))
        {
            return "Energy Finance";
        }

        if (ContainsAny(signal, "bank", "banking", "financial sector", "privatization"))
        {
            return "Banking";
        }

        if (ContainsAny(signal, "regulation", "regulatory", "supervis", "reporting", "compliance"))
        {
            return "Regulation";
        }

        return string.IsNullOrWhiteSpace(topic)
            ? (string.IsNullOrWhiteSpace(type) ? "Banking" : type.Trim())
            : topic.Trim();
    }

    private static string? NormalizeExplicitTopic(string? topic)
    {
        if (string.IsNullOrWhiteSpace(topic))
        {
            return null;
        }

        var value = topic.Trim();

        if (value.Equals("TRADE & LOGISTICS", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("trade", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("logistics", StringComparison.OrdinalIgnoreCase))
        {
            return "Trade & Logistics";
        }

        if (value.Contains("sanction", StringComparison.OrdinalIgnoreCase))
        {
            return "Sanctions";
        }

        if (value.Contains("macro", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("inflation", StringComparison.OrdinalIgnoreCase))
        {
            return "Macroeconomics";
        }

        if (value.Contains("energy", StringComparison.OrdinalIgnoreCase))
        {
            return "Energy Finance";
        }

        if (value.Contains("regulation", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("regulatory", StringComparison.OrdinalIgnoreCase))
        {
            return "Regulation";
        }

        if (value.Contains("bank", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("financial sector", StringComparison.OrdinalIgnoreCase))
        {
            return "Banking";
        }

        return value;
    }

    private static bool ContainsAny(string source, params string[] terms) =>
        terms.Any(source.Contains);
}
