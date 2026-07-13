using CAFRI.ViewModels.Shared;

namespace CAFRI.Infrastructure.Content;

public static class ContentImageTextSerializer
{
    public static IReadOnlyList<ContentImageItemViewModel> Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => line.Split('|').Select(part => part.Trim()).ToArray())
            .Where(parts => parts.Length != 0 && !string.IsNullOrWhiteSpace(parts[0]))
            .Select(parts => new ContentImageItemViewModel
            {
                ImageUrl = parts[0],
                Alt = parts.Length > 1 ? parts[1] : string.Empty,
                Title = parts.Length > 2 ? parts[2] : string.Empty,
                Caption = parts.Length > 3 ? string.Join(" | ", parts.Skip(3)) : string.Empty
            })
            .ToList();
    }

    public static string Serialize(IEnumerable<ContentImageItemViewModel> items) =>
        string.Join(Environment.NewLine, items
            .Where(item => !string.IsNullOrWhiteSpace(item.ImageUrl))
            .Select(item => $"{item.ImageUrl.Trim()} | {item.Alt.Trim()} | {item.Title.Trim()} | {item.Caption.Trim()}"));
}
