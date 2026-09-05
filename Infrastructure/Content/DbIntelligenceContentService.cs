using System.Text.Json;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Intelligence;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public sealed class DbIntelligenceContentService : IIntelligenceContentService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;

    public DbIntelligenceContentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IntelligencePageViewModel> GetIndexPageAsync(
        string? search = null,
        string? country = null,
        string? category = null,
        string? institution = null,
        string? dateRange = null,
        string? sortBy = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 12;
        var countries = await _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var categories = await _dbContext.ContentCategories
            .AsNoTracking()
            .Where(x => x.IsPublished && (x.Scope == "Intelligence" || x.Scope == "Shared"))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var institutions = await _dbContext.ContentSources
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.SourceType)
            .Select(x => x.SourceType)
            .Distinct()
            .ToListAsync(cancellationToken);

        var publishedItemsQuery = _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished);

        var totalPublishedItems = await publishedItemsQuery.CountAsync(cancellationToken);
        var coveredCountriesCount = await publishedItemsQuery
            .Select(x => x.CountryName)
            .Distinct()
            .CountAsync(cancellationToken);
        var usedCategoriesCount = await publishedItemsQuery
            .Select(x => x.Category)
            .Distinct()
            .CountAsync(cancellationToken);
        var monitoredSourcesCount = await publishedItemsQuery
            .Select(x => x.Source)
            .Distinct()
            .CountAsync(cancellationToken);

        var selectedCountry = string.IsNullOrWhiteSpace(country) ? "All Countries" : country.Trim();
        var selectedCategory = string.IsNullOrWhiteSpace(category) ? "All Categories" : category.Trim();
        var selectedInstitution = string.IsNullOrWhiteSpace(institution) ? "All Institutions" : institution.Trim();
        var selectedDateRange = string.IsNullOrWhiteSpace(dateRange) ? "Any time" : dateRange.Trim();
        var selectedSort = string.IsNullOrWhiteSpace(sortBy) ? "Newest" : sortBy.Trim();

        var query = _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{term}%") ||
                EF.Functions.ILike(x.Description, $"%{term}%") ||
                EF.Functions.ILike(x.Source, $"%{term}%") ||
                EF.Functions.ILike(x.Category, $"%{term}%") ||
                EF.Functions.ILike(x.CountryName, $"%{term}%"));
        }

        if (!string.Equals(selectedCountry, "All Countries", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.CountryName == selectedCountry);
        }

        if (!string.Equals(selectedCategory, "All Categories", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(x => x.Category == selectedCategory);
        }

        if (!string.Equals(selectedInstitution, "All Institutions", StringComparison.OrdinalIgnoreCase))
        {
            var institutionSourceNames = await _dbContext.ContentSources
                .AsNoTracking()
                .Where(x => x.IsPublished && x.SourceType == selectedInstitution)
                .Select(x => x.Name)
                .ToListAsync(cancellationToken);

            query = institutionSourceNames.Count == 0
                ? query.Where(_ => false)
                : query.Where(x => institutionSourceNames.Contains(x.Source));
        }

        if (!string.Equals(selectedDateRange, "Any time", StringComparison.OrdinalIgnoreCase))
        {
            var now = DateTimeOffset.UtcNow;
            var cutoff = selectedDateRange switch
            {
                "Last 7 days" => now.AddDays(-7),
                "Last 30 days" => now.AddDays(-30),
                "Last 90 days" => now.AddDays(-90),
                _ => (DateTimeOffset?)null
            };

            if (cutoff is not null)
            {
                query = query.Where(x => x.UpdatedAtUtc >= cutoff.Value);
            }
        }

        query = selectedSort switch
        {
            "Oldest" => query.OrderByDescending(x => x.DisplayOrder).ThenBy(x => x.Title),
            "Country" => query.OrderBy(x => x.CountryName).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Title),
            _ => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Title)
        };

        var totalResults = await query.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalResults / (double)pageSize));
        var currentPage = Math.Min(Math.Max(page, 1), totalPages);

        var items = await query
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new IntelligenceItemCardViewModel
            {
                Slug = x.Slug,
                Category = x.Category,
                CountryCode = x.CountryCode,
                CountryName = x.CountryName,
                PublishedLabel = x.PublishedLabel,
                Title = x.Title,
                Description = x.Description,
                Source = x.Source
            })
            .ToListAsync(cancellationToken);

        return new IntelligencePageViewModel
        {
            Description = "Structured intelligence on regulatory, banking, trade, sanctions, and macroeconomic developments across Central Asia and Eurasian corridors.",
            SearchPlaceholder = "Search intelligence...",
            CountrySelectOptions = ["All Countries", .. countries],
            CategorySelectOptions = ["All Categories", .. categories],
            InstitutionSelectOptions = ["All Institutions", .. institutions],
            SidebarCountries = [new() { Label = "All Countries", Value = "all", IsSelected = string.Equals(selectedCountry, "All Countries", StringComparison.OrdinalIgnoreCase) }, .. countries.Select(x => new IntelligenceFilterOptionViewModel { Label = x, Value = x.ToLowerInvariant().Replace(' ', '-'), IsSelected = x == selectedCountry })],
            SidebarCategories = [new() { Label = "All Categories", Value = "all", IsSelected = string.Equals(selectedCategory, "All Categories", StringComparison.OrdinalIgnoreCase) }, .. categories.Select(x => new IntelligenceFilterOptionViewModel { Label = x, Value = x.ToLowerInvariant().Replace(' ', '-'), IsSelected = x == selectedCategory })],
            DateRangeOptions = ["Any time", "Last 7 days", "Last 30 days", "Last 90 days"],
            Items = items,
            SummaryMetrics =
            [
                new() { Value = totalPublishedItems.ToString(), Label = "Published Items" },
                new() { Value = coveredCountriesCount.ToString(), Label = "Countries Covered" },
                new() { Value = usedCategoriesCount.ToString(), Label = "Core Categories" },
                new() { Value = monitoredSourcesCount.ToString(), Label = "Sources Referenced" }
            ],
            SearchTerm = string.IsNullOrWhiteSpace(search) ? null : search.Trim(),
            CountryFilter = selectedCountry,
            CategoryFilter = selectedCategory,
            InstitutionFilter = selectedInstitution,
            DateRangeFilter = selectedDateRange,
            ShowingFrom = items.Count == 0 ? 0 : 1,
            ShowingTo = items.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + items.Count,
            TotalResults = totalResults,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            PageSize = pageSize,
            SortBy = selectedSort,
            ProfessionalAccessBenefits =
            [
                "Full Regulatory Timeline",
                "Historical Analysis",
                "AI Executive Briefs",
                "Interactive Dashboards",
                "Full Publication Library",
                "Export (PDF & Excel)"
            ]
        };
    }

    public async Task<IntelligenceDetailsViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Include(x => x.Sections)
            .Include(x => x.KeyChangeEntries)
            .Include(x => x.ImpactEntries)
            .Include(x => x.TimelineEntries)
            .Include(x => x.DocumentInfoEntries)
            .Include(x => x.OfficialDocuments)
            .Include(x => x.RelatedLinks)
            .Include(x => x.StatusEntries)
            .Include(x => x.HighlightEntries)
            .FirstOrDefaultAsync(x => x.IsPublished && x.Slug == slug, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        if (HasStructuredDetails(entity))
        {
            return BuildStructuredDetails(entity);
        }

        if (!string.IsNullOrWhiteSpace(entity.DetailsJson))
        {
            try
            {
                var details = JsonSerializer.Deserialize<IntelligenceDetailsViewModel>(entity.DetailsJson, JsonOptions);
                if (details is not null)
                {
                    return details;
                }
            }
            catch (JsonException)
            {
                // Fall through to the generated fallback below.
            }
        }

        return BuildFallbackDetails(entity);
    }

    private static string GetHeroImageClassName(string? countryCode) => countryCode switch
    {
        "UZ" => "article-hero-image--uzbekistan",
        "KG" => "article-hero-image--kyrgyzstan",
        "TJ" => "article-hero-image--tajikistan",
        "TM" => "article-hero-image--turkmenistan",
        _ => "article-hero-image--kazakhstan"
    };

    private static bool HasStructuredDetails(Domain.Content.IntelligenceContentItem item) =>
        item.Sections.Count != 0 ||
        item.KeyChangeEntries.Count != 0 ||
        item.ImpactEntries.Count != 0 ||
        item.TimelineEntries.Count != 0 ||
        item.DocumentInfoEntries.Count != 0 ||
        item.OfficialDocuments.Count != 0 ||
        item.RelatedLinks.Count != 0 ||
        item.StatusEntries.Count != 0 ||
        item.HighlightEntries.Count != 0;

    private static IntelligenceDetailsViewModel BuildStructuredDetails(Domain.Content.IntelligenceContentItem item)
    {
        var overviewLead = item.Sections
            .Where(x => x.SectionKey == "overview")
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.Body)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? item.Description;

        var overviewBody = item.Sections
            .Where(x => x.SectionKey == "overview")
            .OrderBy(x => x.DisplayOrder)
            .Skip(1)
            .Select(x => x.Body)
            .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
            ?? "Additional structured analysis can be expanded through the CAFRI admin content layer.";

        var officialDocument = item.OfficialDocuments
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new IntelligenceOfficialDocumentViewModel
            {
                Title = x.Title,
                Subtitle = x.Subtitle,
                Meta = x.Meta,
                DownloadUrl = x.DownloadUrl ?? "#"
            })
            .FirstOrDefault()
            ?? new IntelligenceOfficialDocumentViewModel
            {
                Title = "Official document is not available yet",
                Subtitle = "Document metadata can be added through the admin content layer",
                Meta = "Source-based intelligence item",
                DownloadUrl = "#"
            };

        return new IntelligenceDetailsViewModel
        {
            Title = item.Title,
            Slug = item.Slug,
            Category = item.Category,
            CountryCode = item.CountryCode,
            CountryName = item.CountryName,
            ShortDescription = item.Description,
            HeroImageClassName = GetHeroImageClassName(item.CountryCode),
            HeroImageUrl = item.HeroImageUrl ?? string.Empty,
            PublishedDate = item.PublishedLabel,
            ReadingTime = "8 min read",
            AnalysisType = "CAFRI Intelligence Note",
            OverviewLead = overviewLead,
            OverviewBody = overviewBody,
            EffectiveDate = item.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.Label == "Effective Date")?.Value ?? "Monitoring",
            AppliesTo = item.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.Label == "Applies To")?.Value ?? "Relevant stakeholders",
            IsPremium = true,
            Actions =
            [
                new() { Label = "Save", Icon = "save" },
                new() { Label = "Cite", Icon = "cite" },
                new() { Label = "Share", Icon = "share" },
                new() { Label = "Print", Icon = "print" }
            ],
            Tabs =
            [
                new() { Label = "Overview", IsActive = true },
                new() { Label = "Key Changes" },
                new() { Label = "Impact Analysis" },
                new() { Label = "Timeline" },
                new() { Label = "Regulatory Text" },
                new() { Label = "Data & Charts" },
                new() { Label = "Related Materials" }
            ],
            KeyChanges = item.KeyChangeEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceKeyChangeViewModel
                {
                    Number = x.Number,
                    Title = x.Title,
                    Description = x.Description
                })
                .ToList(),
            ImpactAnalysis = item.ImpactEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceImpactCardViewModel
                {
                    Title = x.Title,
                    Description = x.Description,
                    ImpactLabel = x.ImpactLabel,
                    ImpactTone = x.ImpactTone
                })
                .ToList(),
            TimelineItems = item.TimelineEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceTimelineItemViewModel
                {
                    DateLabel = x.DateLabel,
                    Description = x.Description,
                    Stage = x.Stage
                })
                .ToList(),
            OfficialDocument = officialDocument,
            AvailableDownloads = item.OfficialDocuments
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceOfficialDocumentViewModel
                {
                    Title = x.Title,
                    Subtitle = x.Subtitle,
                    Meta = x.Meta,
                    DownloadUrl = x.DownloadUrl ?? "#"
                })
                .ToList(),
            DocumentInformation = item.DocumentInfoEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceDocumentInfoRowViewModel
                {
                    Label = x.Label,
                    Value = x.Value
                })
                .ToList(),
            KeyHighlights = item.HighlightEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Text)
                .ToList(),
            RelatedOfficialSources = item.RelatedLinks
                .Where(x => x.LinkType == "OfficialSource")
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceSidebarLinkViewModel
                {
                    Title = x.Title,
                    Subtitle = x.Subtitle,
                    Url = x.Url
                })
                .ToList(),
            RelatedIntelligence = item.RelatedLinks
                .Where(x => x.LinkType == "RelatedIntelligence")
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceSidebarLinkViewModel
                {
                    Title = x.Title,
                    Subtitle = x.Subtitle,
                    Url = x.Url
                })
                .ToList(),
            MonitoringStatus = item.StatusEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new IntelligenceStatusRowViewModel
                {
                    Label = x.Label,
                    Value = x.Value,
                    IsStatus = x.IsStatus
                })
                .ToList()
        };
    }

    private static IntelligenceDetailsViewModel BuildFallbackDetails(Domain.Content.IntelligenceContentItem item) =>
        new()
        {
            Title = item.Title,
            Slug = item.Slug,
            Category = item.Category,
            CountryCode = item.CountryCode,
            CountryName = item.CountryName,
            ShortDescription = item.Description,
            HeroImageClassName = GetHeroImageClassName(item.CountryCode),
            HeroImageUrl = item.HeroImageUrl ?? string.Empty,
            PublishedDate = item.PublishedLabel,
            ReadingTime = "8 min read",
            AnalysisType = "CAFRI Intelligence Note",
            OverviewLead = $"{item.Title} is currently available as a structured intelligence note within the CAFRI platform.",
            OverviewBody = "This fallback detail page preserves the analytical layout and gives the project a consistent database-driven path while richer material is still being added through the admin area.",
            EffectiveDate = "Monitoring",
            AppliesTo = "Relevant public and private sector stakeholders",
            IsPremium = true,
            Actions = [ new() { Label = "Save", Icon = "save" }, new() { Label = "Cite", Icon = "cite" }, new() { Label = "Share", Icon = "share" }, new() { Label = "Print", Icon = "print" } ],
            Tabs = [ new() { Label = "Overview", IsActive = true }, new() { Label = "Key Changes" }, new() { Label = "Impact Analysis" }, new() { Label = "Timeline" }, new() { Label = "Regulatory Text" }, new() { Label = "Data & Charts" }, new() { Label = "Related Materials" } ],
            KeyChanges = [ new() { Number = 1, Title = "Database-backed record", Description = "This intelligence item is now served from the CMS tables instead of controller hardcode." }, new() { Number = 2, Title = "Admin editable", Description = "Editors can update title, summary, source metadata, and full detail JSON through the admin area." }, new() { Number = 3, Title = "Expandable detail model", Description = "The page supports progressive enrichment without changing routing or controller logic." } ],
            ImpactAnalysis = [ new() { Title = "Editors", Description = "Content can now be maintained through the admin panel.", ImpactLabel = "Impact: Positive", ImpactTone = "impact-positive" }, new() { Title = "Platform", Description = "The public layer now reads this item from the database at runtime.", ImpactLabel = "Impact: High", ImpactTone = "impact-high" }, new() { Title = "Users", Description = "Users get a stable detail page even when a custom deep-dive has not been authored yet.", ImpactLabel = "Impact: Medium", ImpactTone = "impact-medium" } ],
            TimelineItems = [ new() { DateLabel = item.PublishedLabel, Description = "Intelligence item published", Stage = "past" }, new() { DateLabel = "Current", Description = "Content monitored in CAFRI", Stage = "current" }, new() { DateLabel = "Next update", Description = "Full detail can be expanded from the admin editor", Stage = "future" } ],
            OfficialDocument = new IntelligenceOfficialDocumentViewModel { Title = "Official document or source reference", Subtitle = item.Source, Meta = $"{item.PublishedLabel} | Source-based item" },
            AvailableDownloads = [],
            DocumentInformation = [ new() { Label = "Category", Value = item.Category }, new() { Label = "Country", Value = item.CountryName }, new() { Label = "Published", Value = item.PublishedLabel }, new() { Label = "Source", Value = item.Source }, new() { Label = "Status", Value = "Monitoring" } ],
            KeyHighlights = [ "Database-backed content source in place", "Fallback detail view available for unpublished deep dives", "Ready for progressive enrichment by editors" ],
            RelatedOfficialSources = [ new() { Title = item.Source, Subtitle = "Primary source reference" } ],
            RelatedIntelligence = [ new() { Title = "More intelligence items", Subtitle = "Browse the full catalog", Url = "/intelligence" } ],
            MonitoringStatus = [ new() { Label = "Status", Value = "Monitoring", IsStatus = true }, new() { Label = "Source", Value = item.Source }, new() { Label = "Coverage", Value = item.CountryName } ]
        };
}
