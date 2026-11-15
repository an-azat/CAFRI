using System.Text.Json;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Intelligence;
using CAFRI.ViewModels.Shared;
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

    public async Task<IntelligencePageViewModel> GetArchivePageAsync(
        string? search = null,
        string? country = null,
        string? category = null,
        string? institution = null,
        string? dateRange = null,
        string? sortBy = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 9;
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
            // Items store categories in display case (e.g. "FINANCIAL REGULATION") while the
            // category list uses title case, so compare case-insensitively.
            var normalizedCategory = selectedCategory.ToLower();
            query = query.Where(x => x.Category.ToLower() == normalizedCategory);
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
                query = query.Where(x => x.PublishedAtUtc >= cutoff.Value);
            }
        }

        query = selectedSort switch
        {
            // Date sorts use the publication date; undated items go last, DisplayOrder breaks ties.
            "Oldest" => query.OrderBy(x => x.PublishedAtUtc == null).ThenBy(x => x.PublishedAtUtc).ThenBy(x => x.DisplayOrder),
            "Country" => query.OrderBy(x => x.CountryName).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Title),
            _ => query.OrderBy(x => x.PublishedAtUtc == null).ThenByDescending(x => x.PublishedAtUtc).ThenBy(x => x.DisplayOrder)
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
                PublishedLabel = PublicationMetadataFormatter.FormatShortDate(x.PublishedAtUtc),
                Title = x.Title,
                Description = x.Description,
                Source = x.Source,
                ImageUrl = x.HeroImageUrl
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
            SortBy = selectedSort
        };
    }

    public async Task<IntelligenceLandingViewModel> GetLandingPageAsync(CancellationToken cancellationToken = default)
    {
        const int leadItemCount = 3;
        const int itemsPerCountry = 2;
        const int trendingItemCount = 4;

        // "Latest" follows the same order as the archive's default "Newest" sort.
        var items = await _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.PublishedAtUtc == null)
            .ThenByDescending(x => x.PublishedAtUtc)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => new IntelligenceItemCardViewModel
            {
                Slug = x.Slug,
                Category = x.Category,
                CountryCode = x.CountryCode,
                CountryName = x.CountryName,
                PublishedLabel = PublicationMetadataFormatter.FormatShortDate(x.PublishedAtUtc),
                Title = x.Title,
                Description = x.Description,
                Source = x.Source,
                ImageUrl = x.HeroImageUrl
            })
            .ToListAsync(cancellationToken);

        // Trending Now is an editorial pick made in the admin, not a popularity metric.
        var trendingSlugs = await _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished && x.IsTrending)
            .OrderBy(x => x.TrendingOrder)
            .ThenBy(x => x.DisplayOrder)
            .Select(x => x.Slug)
            .Take(trendingItemCount)
            .ToListAsync(cancellationToken);

        var countries = await _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Select(x => new { x.Code, x.Name })
            .ToListAsync(cancellationToken);

        return new IntelligenceLandingViewModel
        {
            TotalPublished = items.Count,
            LeadItem = items.FirstOrDefault(),
            SecondaryItems = items.Skip(1).Take(leadItemCount - 1).ToList(),
            Countries = countries
                .Select(country =>
                {
                    var countryItems = items
                        .Where(x => string.Equals(x.CountryCode, country.Code, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    return new IntelligenceCountryDigestViewModel
                    {
                        Code = country.Code.ToUpperInvariant(),
                        Name = country.Name,
                        TotalItems = countryItems.Count,
                        LatestItems = countryItems.Take(itemsPerCountry).ToList()
                    };
                })
                .ToList(),
            TrendingItems = trendingSlugs
                .Select(slug => items.First(x => x.Slug == slug))
                .ToList()
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

        var details = BuildDetails(entity);
        details.RelatedItems = await GetRelatedItemsAsync(entity, cancellationToken);
        return details;
    }

    private IntelligenceDetailsViewModel BuildDetails(Domain.Content.IntelligenceContentItem entity)
    {
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

    // Other published items, same category first, for the sidebar of the guest view.
    private async Task<IReadOnlyList<IntelligenceItemCardViewModel>> GetRelatedItemsAsync(
        Domain.Content.IntelligenceContentItem entity,
        CancellationToken cancellationToken)
    {
        const int relatedItemCount = 3;
        var category = entity.Category.ToLower();

        return await _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .Where(x => x.IsPublished && x.Id != entity.Id)
            .OrderBy(x => x.Category.ToLower() == category ? 0 : 1)
            .ThenBy(x => x.DisplayOrder)
            .ThenBy(x => x.Title)
            .Take(relatedItemCount)
            .Select(x => new IntelligenceItemCardViewModel
            {
                Slug = x.Slug,
                Category = x.Category,
                CountryCode = x.CountryCode,
                CountryName = x.CountryName,
                PublishedLabel = PublicationMetadataFormatter.FormatShortDate(x.PublishedAtUtc),
                Title = x.Title,
                Description = x.Description,
                Source = x.Source
            })
            .ToListAsync(cancellationToken);
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
            ?? string.Empty;

        var officialDocument = item.OfficialDocuments
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new IntelligenceOfficialDocumentViewModel
            {
                Title = x.Title,
                Subtitle = x.Subtitle,
                Meta = x.Meta,
                DownloadUrl = DocumentLink.Url(x.DownloadUrl)
            })
            .FirstOrDefault()
            ?? new IntelligenceOfficialDocumentViewModel { Title = string.Empty, Subtitle = string.Empty, Meta = string.Empty };

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
            PublishedDate = PublicationMetadataFormatter.FormatShortDate(item.PublishedAtUtc),
            ReadingTime = "8 min read",
            AnalysisType = "CAFRI Intelligence Note",
            OverviewLead = overviewLead,
            OverviewBody = overviewBody,
            EffectiveDate = item.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.Label == "Effective Date")?.Value ?? string.Empty,
            AppliesTo = item.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.Label == "Applies To")?.Value ?? string.Empty,
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
                    DownloadUrl = DocumentLink.Url(x.DownloadUrl)
                })
                .ToList(),
            // Drop repeated label/value pairs left behind by the old save bug.
            DocumentInformation = item.DocumentInfoEntries
                .OrderBy(x => x.DisplayOrder)
                .DistinctBy(x => (x.Label.Trim().ToLowerInvariant(), x.Value.Trim()))
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
            PublishedDate = PublicationMetadataFormatter.FormatShortDate(item.PublishedAtUtc),
            ReadingTime = "8 min read",
            AnalysisType = "CAFRI Intelligence Note",
            // No detailed content was authored for this item: show only what the record
            // actually holds. Empty blocks are hidden by the view.
            OverviewLead = item.Description,
            OverviewBody = string.Empty,
            EffectiveDate = string.Empty,
            AppliesTo = string.Empty,
            IsPremium = true,
            Actions = [ new() { Label = "Save", Icon = "save" }, new() { Label = "Cite", Icon = "cite" }, new() { Label = "Share", Icon = "share" }, new() { Label = "Print", Icon = "print" } ],
            Tabs = [ new() { Label = "Overview", IsActive = true }, new() { Label = "Key Changes" }, new() { Label = "Impact Analysis" }, new() { Label = "Timeline" }, new() { Label = "Regulatory Text" }, new() { Label = "Data & Charts" }, new() { Label = "Related Materials" } ],
            KeyChanges = [],
            ImpactAnalysis = [],
            TimelineItems = [],
            OfficialDocument = new IntelligenceOfficialDocumentViewModel { Title = string.Empty, Subtitle = string.Empty, Meta = string.Empty },
            AvailableDownloads = [],
            DocumentInformation = [ new() { Label = "Category", Value = item.Category }, new() { Label = "Country", Value = item.CountryName }, new() { Label = "Published", Value = PublicationMetadataFormatter.FormatShortDate(item.PublishedAtUtc) }, new() { Label = "Source", Value = item.Source }, new() { Label = "Status", Value = "Monitoring" } ],
            KeyHighlights = [],
            RelatedOfficialSources = [ new() { Title = item.Source, Subtitle = "Primary source reference" } ],
            RelatedIntelligence = [ new() { Title = "More intelligence items", Subtitle = "Browse the full catalog", Url = "/intelligence/archive" } ],
            MonitoringStatus = [ new() { Label = "Status", Value = "Monitoring", IsStatus = true }, new() { Label = "Source", Value = item.Source }, new() { Label = "Coverage", Value = item.CountryName } ]
        };
}
