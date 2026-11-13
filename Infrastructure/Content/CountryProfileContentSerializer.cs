using System.Text.Json;
using CAFRI.Domain.Content;

namespace CAFRI.Infrastructure.Content;

public static class CountryProfileContentSerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Missing or unreadable JSON yields an empty profile, so the page and the admin form still open.
    public static CountryProfileContent Parse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return new CountryProfileContent();
        }

        try
        {
            return JsonSerializer.Deserialize<CountryProfileContent>(json, JsonOptions) ?? new CountryProfileContent();
        }
        catch (JsonException)
        {
            return new CountryProfileContent();
        }
    }

    public static string Serialize(CountryProfileContent content)
    {
        ReplaceNullStrings(content);
        content.RemoveEmptyRows();
        return JsonSerializer.Serialize(content);
    }

    // Model binding turns empty inputs into null; store them as empty strings so the page code
    // can treat every field as a plain (possibly empty) string.
    private static void ReplaceNullStrings(object target)
    {
        foreach (var property in target.GetType().GetProperties().Where(x => x.CanRead && x.CanWrite))
        {
            if (property.PropertyType == typeof(string))
            {
                property.SetValue(target, property.GetValue(target) ?? string.Empty);
            }
            else if (property.GetValue(target) is System.Collections.IEnumerable items and not string)
            {
                foreach (var item in items)
                {
                    if (item is not null && item.GetType().IsClass)
                    {
                        ReplaceNullStrings(item);
                    }
                }
            }
        }
    }
}
