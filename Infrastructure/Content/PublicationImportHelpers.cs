using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CAFRI.Domain.Content;
using CAFRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public static class PublicationImportHelpers
{
    public static string NormalizeSourceUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var raw = value.Trim();
        if (!Uri.TryCreate(raw, UriKind.Absolute, out var uri))
        {
            return raw.TrimEnd('/').ToLowerInvariant();
        }

        var builder = new UriBuilder(uri)
        {
            Query = string.Empty,
            Fragment = string.Empty
        };

        return builder.Uri.AbsoluteUri.TrimEnd('/').ToLowerInvariant();
    }

    public static string ExtractDomain(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri))
        {
            return uri.Host.ToLowerInvariant();
        }

        return value.Trim().ToLowerInvariant();
    }

    public static string GenerateSlug(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Guid.NewGuid().ToString("N");
        }

        var normalized = title.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        var previousDash = false;

        foreach (var character in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsLetterOrDigit(character))
            {
                builder.Append(character);
                previousDash = false;
                continue;
            }

            if (previousDash)
            {
                continue;
            }

            builder.Append('-');
            previousDash = true;
        }

        var slug = Regex.Replace(builder.ToString().Trim('-'), "-{2,}", "-");
        return string.IsNullOrWhiteSpace(slug) ? Guid.NewGuid().ToString("N") : slug;
    }

    public static IReadOnlyList<string> ParseDelimitedValues(string? raw) =>
        (raw ?? string.Empty)
            .Split([',', ';', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public static async Task<List<PublicationCountryAssignment>> ResolveCountryAssignmentsAsync(
        AppDbContext dbContext,
        IEnumerable<string> values,
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        var normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            return [];
        }

        var countries = await dbContext.CountryContents
            .AsNoTracking()
            .Select(x => new { x.Code, x.Name })
            .ToListAsync(cancellationToken);

        var result = new List<PublicationCountryAssignment>();

        for (var index = 0; index < normalized.Count; index++)
        {
            var value = normalized[index];
            if (string.Equals(value, "regional", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "rg", StringComparison.OrdinalIgnoreCase))
            {
                result.Add(new PublicationCountryAssignment
                {
                    Id = Guid.NewGuid(),
                    PublicationContentItemId = publicationId,
                    CountryCode = "RG",
                    CountryLabel = "Regional",
                    DisplayOrder = index
                });

                continue;
            }

            var match = countries.FirstOrDefault(country =>
                string.Equals(country.Code, value, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(country.Name, value, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                continue;
            }

            result.Add(new PublicationCountryAssignment
            {
                Id = Guid.NewGuid(),
                PublicationContentItemId = publicationId,
                CountryCode = match.Code,
                CountryLabel = match.Name,
                DisplayOrder = index
            });
        }

        return result
            .GroupBy(item => item.CountryCode, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.DisplayOrder)
            .ToList();
    }

    public static async Task<List<PublicationCategoryAssignment>> ResolveCategoryAssignmentsAsync(
        AppDbContext dbContext,
        IEnumerable<string> values,
        Guid publicationId,
        CancellationToken cancellationToken)
    {
        var normalized = values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalized.Count == 0)
        {
            return [];
        }

        var categories = await dbContext.ContentCategories
            .AsNoTracking()
            .Where(x => x.Scope == "Publications" || x.Scope == "Shared")
            .Select(x => new { x.Name, x.Slug })
            .ToListAsync(cancellationToken);

        var result = new List<PublicationCategoryAssignment>();

        for (var index = 0; index < normalized.Count; index++)
        {
            var value = normalized[index];
            var match = categories.FirstOrDefault(category =>
                string.Equals(category.Name, value, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(category.Slug, value, StringComparison.OrdinalIgnoreCase));

            if (match is null)
            {
                continue;
            }

            result.Add(new PublicationCategoryAssignment
            {
                Id = Guid.NewGuid(),
                PublicationContentItemId = publicationId,
                CategoryName = match.Name,
                CategorySlug = match.Slug,
                DisplayOrder = index
            });
        }

        return result
            .GroupBy(item => item.CategorySlug, StringComparer.OrdinalIgnoreCase)
            .Select(group => group.First())
            .OrderBy(item => item.DisplayOrder)
            .ToList();
    }
}
