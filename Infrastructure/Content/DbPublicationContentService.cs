using System.Text.Json;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Domain.Content;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Publications;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Content;

public sealed class DbPublicationContentService : IPublicationContentService
{
    private static readonly IReadOnlyList<PublicationActionItemViewModel> DefaultActions =
    [
        new() { Label = "Download Full Report" },
        new() { Label = "Share" },
        new() { Label = "Print" }
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;

    private static bool LooksLikeHtml(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        value.Contains('<', StringComparison.Ordinal) &&
        value.Contains('>', StringComparison.Ordinal);

    private static bool HasExplicitJsonCollection(string? rawJson, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(rawJson);
            return document.RootElement.TryGetProperty(propertyName, out var property) &&
                   property.ValueKind == JsonValueKind.Array;
        }
        catch
        {
            return false;
        }
    }

    private static string SanitizeRichTextHtml(string? html) => RichTextHtmlSanitizer.Sanitize(html);

    public DbPublicationContentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static IReadOnlyList<PublicationGalleryItemViewModel> ParseGalleryItems(string? raw)
    {
        var items = ContentImageTextSerializer.Parse(raw);
        return items.Select(item => new PublicationGalleryItemViewModel
        {
            ImageUrl = item.ImageUrl,
            Alt = item.Alt,
            Title = string.IsNullOrWhiteSpace(item.Title) ? "Gallery image" : item.Title,
            Caption = item.Caption,
            VisualClassName = string.Empty
        }).ToList();
    }

    private static IReadOnlyList<string> BuildCountryLabels(Domain.Content.PublicationContentItem item)
    {
        var labels = item.CountryAssignments
            .Select(x => x.CountryLabel)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        return labels.Count == 0 ? [item.CountryLabel] : labels;
    }

    private static IReadOnlyList<string> BuildTopicLabels(Domain.Content.PublicationContentItem item)
    {
        var labels = item.CategoryAssignments
            .Select(x => x.CategoryName)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        if (labels.Count != 0)
        {
            return labels;
        }

        return string.IsNullOrWhiteSpace(item.Topic) ? [item.Type] : [item.Topic];
    }

    private static IQueryable<Domain.Content.PublicationContentItem> ApplyTopicFilter(
        IQueryable<Domain.Content.PublicationContentItem> query,
        string selectedTopic)
    {
        if (string.Equals(selectedTopic, "Trade & Logistics", StringComparison.OrdinalIgnoreCase))
        {
            return query.Where(x =>
                EF.Functions.ILike(x.Topic, selectedTopic) ||
                x.Type == "REPORT");
        }

        return query.Where(x =>
            EF.Functions.ILike(x.Topic, selectedTopic) ||
            EF.Functions.ILike(x.Type, selectedTopic));
    }

    private static bool MatchesTopicCategory(Domain.Content.PublicationContentItem item, string selectedTopic)
    {
        if (string.Equals(selectedTopic, "Trade & Logistics", StringComparison.OrdinalIgnoreCase))
        {
            return string.Equals(item.Topic, "Trade & Logistics", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(item.Topic, "TRADE & LOGISTICS", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(item.Type, "REPORT", StringComparison.OrdinalIgnoreCase);
        }

        return string.Equals(item.Topic, selectedTopic, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(item.Type, selectedTopic, StringComparison.OrdinalIgnoreCase);
    }

    public async Task<PublicationsPageViewModel> GetIndexPageAsync(
        string? query = null,
        string? type = null,
        string? country = null,
        string? topic = null,
        string? tab = null,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 9;

        var categories = await _dbContext.ContentCategories
            .AsNoTracking()
            .Where(x => x.IsPublished && (x.Scope == "Publications" || x.Scope == "Shared"))
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var countries = await _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Name)
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var selectedType = string.IsNullOrWhiteSpace(type) ? "All Types" : type.Trim();
        var selectedCountry = string.IsNullOrWhiteSpace(country) ? "All Countries" : country.Trim();
        var selectedTopic = string.IsNullOrWhiteSpace(topic) ? "All Topics" : topic.Trim();
        var selectedTab = string.IsNullOrWhiteSpace(tab) ? "latest" : tab.Trim().ToLowerInvariant();

        var publicationsQuery = _dbContext.PublicationContentItems
            .AsNoTracking()
            .Include(x => x.CountryAssignments)
            .Include(x => x.CategoryAssignments)
            .Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Published)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim();
            publicationsQuery = publicationsQuery.Where(x =>
                EF.Functions.ILike(x.Title, $"%{term}%") ||
                EF.Functions.ILike(x.Description, $"%{term}%") ||
                EF.Functions.ILike(x.Type, $"%{term}%") ||
                EF.Functions.ILike(x.CountryLabel, $"%{term}%") ||
                EF.Functions.ILike(x.Topic, $"%{term}%"));
        }

        if (!string.Equals(selectedType, "All Types", StringComparison.OrdinalIgnoreCase))
        {
            publicationsQuery = publicationsQuery.Where(x => x.Type == selectedType);
        }

        if (!string.Equals(selectedCountry, "All Countries", StringComparison.OrdinalIgnoreCase))
        {
            publicationsQuery = selectedCountry == "Regional"
                ? publicationsQuery.Where(x => x.CountryLabel == "Regional" || x.CountryAssignments.Any(item => item.CountryCode == "RG" || item.CountryLabel == "Regional"))
                : publicationsQuery.Where(x => x.CountryLabel == selectedCountry || x.CountryAssignments.Any(item => item.CountryLabel == selectedCountry));
        }

        if (!string.Equals(selectedTopic, "All Topics", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(selectedTopic, "Trade & Logistics", StringComparison.OrdinalIgnoreCase))
            {
                publicationsQuery = publicationsQuery.Where(x =>
                    EF.Functions.ILike(x.Topic, selectedTopic) ||
                    x.Type == "REPORT" ||
                    x.CategoryAssignments.Any(item => item.CategoryName == selectedTopic || item.CategorySlug == "trade-logistics"));
            }
            else
            {
                publicationsQuery = publicationsQuery.Where(x =>
                    EF.Functions.ILike(x.Topic, selectedTopic) ||
                    EF.Functions.ILike(x.Type, selectedTopic) ||
                    x.CategoryAssignments.Any(item => item.CategoryName == selectedTopic || item.CategorySlug == selectedTopic));
            }
        }

        publicationsQuery = selectedTab switch
        {
            "most-read" => publicationsQuery.OrderByDescending(x => x.UpdatedAtUtc).ThenBy(x => x.DisplayOrder),
            "by-country" => publicationsQuery.OrderBy(x => x.CountryLabel).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Title),
            "by-topic" => publicationsQuery.OrderBy(x => x.Topic).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Title),
            _ => publicationsQuery.OrderBy(x => x.DisplayOrder).ThenByDescending(x => x.UpdatedAtUtc)
        };

        var totalResults = await publicationsQuery.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalResults / (double)pageSize));
        var currentPage = Math.Min(Math.Max(page, 1), totalPages);

        var publications = await publicationsQuery
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new PublicationCardViewModel
            {
                Type = x.Type,
                CountryCode = x.CountryCode,
                CountryLabel = x.CountryLabel,
                Title = x.Title,
                Description = x.Description,
                Meta = x.Meta,
                Slug = x.Slug
            })
            .ToListAsync(cancellationToken);

        var allPublishedPublications = await _dbContext.PublicationContentItems
            .AsNoTracking()
            .Include(x => x.CountryAssignments)
            .Include(x => x.CategoryAssignments)
            .Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Published)
            .ToListAsync(cancellationToken);

        return new PublicationsPageViewModel
        {
            Query = query?.Trim() ?? string.Empty,
            SelectedType = selectedType,
            SelectedCountry = selectedCountry,
            SelectedTopic = selectedTopic,
            SelectedTab = selectedTab,
            CurrentPage = currentPage,
            TotalPages = totalPages,
            TotalResults = totalResults,
            ShowingFrom = publications.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + 1,
            ShowingTo = publications.Count == 0 ? 0 : ((currentPage - 1) * pageSize) + publications.Count,
            Description = "In-depth reports, country profiles and analytical briefs from the CAFRI team.",
            TypeOptions = ["All Types", .. allPublishedPublications.Select(x => x.Type).Distinct().Order().ToList()],
            CountryOptions = ["All Countries", "Regional", .. countries],
            TopicOptions = ["All Topics", .. categories],
            Tabs =
            [
                new() { Label = "Latest Publications", IsActive = selectedTab == "latest" },
                new() { Label = "Most Read", IsActive = selectedTab == "most-read" },
                new() { Label = "By Country", IsActive = selectedTab == "by-country" },
                new() { Label = "By Topic", IsActive = selectedTab == "by-topic" }
            ],
            Publications = publications,
            Categories =
            [
                new()
                {
                    Icon = "all",
                    Label = "All Publications",
                    Count = allPublishedPublications.Count,
                    Url = "/publications",
                    IsActive = string.Equals(selectedType, "All Types", StringComparison.OrdinalIgnoreCase)
                        && string.Equals(selectedTopic, "All Topics", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "report",
                    Label = "Reports",
                    Count = allPublishedPublications.Count(x => x.Type == "REPORT"),
                    Url = "/publications?type=REPORT",
                    IsActive = string.Equals(selectedType, "REPORT", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "profile",
                    Label = "Country Profiles",
                    Count = allPublishedPublications.Count(x => x.Type == "COUNTRY PROFILE"),
                    Url = "/publications?type=COUNTRY%20PROFILE",
                    IsActive = string.Equals(selectedType, "COUNTRY PROFILE", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "brief",
                    Label = "Analytical Briefs",
                    Count = allPublishedPublications.Count(x => x.Type == "ANALYTICAL BRIEF"),
                    Url = "/publications?type=ANALYTICAL%20BRIEF",
                    IsActive = string.Equals(selectedType, "ANALYTICAL BRIEF", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "note",
                    Label = "Regulatory Notes",
                    Count = allPublishedPublications.Count(x => x.Type == "REGULATORY NOTE"),
                    Url = "/publications?type=REGULATORY%20NOTE",
                    IsActive = string.Equals(selectedType, "REGULATORY NOTE", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "bank",
                    Label = "Banking Reviews",
                    Count = allPublishedPublications.Count(x => x.Type == "BANKING REVIEW"),
                    Url = "/publications?type=BANKING%20REVIEW",
                    IsActive = string.Equals(selectedType, "BANKING REVIEW", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "trade",
                    Label = "Trade & Logistics",
                    Count = allPublishedPublications.Count(x => MatchesTopicCategory(x, "Trade & Logistics")),
                    Url = "/publications?topic=Trade%20%26%20Logistics",
                    IsActive = string.Equals(selectedTopic, "Trade & Logistics", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "shield",
                    Label = "Sanctions Analysis",
                    Count = allPublishedPublications.Count(x => x.Type == "SANCTIONS ANALYSIS"),
                    Url = "/publications?type=SANCTIONS%20ANALYSIS",
                    IsActive = string.Equals(selectedType, "SANCTIONS ANALYSIS", StringComparison.OrdinalIgnoreCase)
                },
                new()
                {
                    Icon = "research",
                    Label = "Research Papers",
                    Count = allPublishedPublications.Count(x => x.Type == "RESEARCH PAPER"),
                    Url = "/publications?type=RESEARCH%20PAPER",
                    IsActive = string.Equals(selectedType, "RESEARCH PAPER", StringComparison.OrdinalIgnoreCase)
                }
            ],
            LatestPublications = allPublishedPublications
                .OrderByDescending(x => x.UpdatedAtUtc)
                .ThenByDescending(x => x.CreatedAtUtc)
                .Take(3)
                .Select(x => new PublicationListItemViewModel
            {
                Title = x.Title,
                Meta = $"{x.Type} | {x.Meta.Split('|')[0].Trim()}"
            }).ToList(),
            Benefits =
            [
                new() { Icon = "expert", Title = "Expert Analysis", Description = "Professionally researched by regional specialists" },
                new() { Icon = "source", Title = "Source-Based", Description = "All intelligence is based on official and reliable sources" },
                new() { Icon = "ai", Title = "AI-Assisted", Description = "Advanced AI tools enhance analysis and monitoring" },
                new() { Icon = "insight", Title = "Actionable Insights", Description = "Providing actionable intelligence for informed decisions" },
                new() { Icon = "updates", Title = "Regular Updates", Description = "Continuous monitoring and timely updates" }
            ]
        };
    }

    public async Task<PublicationDetailsViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.PublicationContentItems
            .AsNoTracking()
            .Include(x => x.ActionEntries)
            .Include(x => x.HighlightEntries)
            .Include(x => x.FindingEntries)
            .Include(x => x.Sections)
            .Include(x => x.DocumentEntries)
            .Include(x => x.RelatedLinks)
            .Include(x => x.InfoEntries)
            .Include(x => x.CountryAssignments)
            .Include(x => x.CategoryAssignments)
            .FirstOrDefaultAsync(x => x.WorkflowStatus == PublicationWorkflowStatuses.Published && x.Slug == slug, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        var relatedCards = await _dbContext.PublicationContentItems
            .AsNoTracking()
            .Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Published && x.Slug != slug)
            .OrderBy(x => x.DisplayOrder)
            .Take(4)
            .Select(x => new PublicationCardViewModel
            {
                Type = x.Type,
                CountryCode = x.CountryCode,
                CountryLabel = x.CountryLabel,
                Title = x.Title,
                Description = x.Description,
                Meta = x.Meta,
                Slug = x.Slug
            })
            .ToListAsync(cancellationToken);

        var fallback = BuildFallbackDetails(entity, relatedCards);
        var jsonDetails = TryDeserializeDetails(entity.DetailsJson);
        var detailsFallback = jsonDetails is null
            ? fallback
            : MergeWithFallback(jsonDetails, fallback, entity.DetailsJson);

        if (HasStructuredDetails(entity))
        {
            return BuildStructuredDetails(entity, detailsFallback);
        }

        if (jsonDetails is not null)
        {
            var merged = detailsFallback;
            merged.HeroImageUrl = entity.HeroImageUrl ?? merged.HeroImageUrl;
            return merged;
        }

        return fallback;
    }

    private static PublicationDetailsViewModel? TryDeserializeDetails(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PublicationDetailsViewModel>(detailsJson, JsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static bool HasStructuredDetails(Domain.Content.PublicationContentItem item) =>
        item.ActionEntries.Count != 0 ||
        item.HighlightEntries.Count != 0 ||
        item.FindingEntries.Count != 0 ||
        item.Sections.Count != 0 ||
        item.DocumentEntries.Count != 0 ||
        item.RelatedLinks.Count != 0 ||
        item.InfoEntries.Count != 0;

    private static PublicationDetailsViewModel BuildStructuredDetails(
        Domain.Content.PublicationContentItem item,
        PublicationDetailsViewModel fallback)
    {
        var summarySection = item.Sections
            .Where(x => x.SectionKey == "executive-summary")
            .OrderBy(x => x.DisplayOrder)
            .FirstOrDefault();

        return new PublicationDetailsViewModel
        {
            Title = item.Title,
            Slug = item.Slug,
            PublicationType = item.Type,
            Topic = string.IsNullOrWhiteSpace(item.Topic) ? fallback.Topic : item.Topic,
            CoverageLabel = item.CountryLabel,
            ShortDescription = item.Description,
            PublishedDate = string.IsNullOrWhiteSpace(item.PublishedDate) ? fallback.PublishedDate : item.PublishedDate,
            ReadingTime = string.IsNullOrWhiteSpace(item.ReadingTime) ? fallback.ReadingTime : item.ReadingTime,
            AuthorLabel = string.IsNullOrWhiteSpace(item.AuthorLabel) ? fallback.AuthorLabel : item.AuthorLabel,
            DocumentLabel = string.IsNullOrWhiteSpace(item.DocumentLabel) ? fallback.DocumentLabel : item.DocumentLabel,
            HeroVisualClassName = string.IsNullOrWhiteSpace(item.HeroVisualClassName) ? fallback.HeroVisualClassName : item.HeroVisualClassName,
            HeroImageUrl = item.HeroImageUrl ?? string.Empty,
            SourceName = string.IsNullOrWhiteSpace(item.SourceName) ? fallback.SourceName : item.SourceName,
            SourceUrl = string.IsNullOrWhiteSpace(item.SourceUrl) ? fallback.SourceUrl : item.SourceUrl,
            CountryLabels = BuildCountryLabels(item),
            TopicLabels = BuildTopicLabels(item),
            PdfDownloadUrl = string.IsNullOrWhiteSpace(item.PdfDownloadUrl) ? fallback.PdfDownloadUrl : item.PdfDownloadUrl,
            ExecutiveSummaryHtml = summarySection is not null && LooksLikeHtml(summarySection.ParagraphsText)
                ? SanitizeRichTextHtml(summarySection.ParagraphsText)
                : string.Empty,
            IsPremium = fallback.IsPremium,
            Actions = DefaultActions.ToList(),
            Tabs = fallback.Tabs,
            Kpis = fallback.Kpis,
            // When the summary was authored as rich HTML (via the admin Quill editor),
            // ParagraphsText holds that HTML rather than "~~"-joined plain-text paragraphs.
            // Parsing it as plain text would dump raw markup into the guest-preview text,
            // so fall back to the generic paragraphs instead.
            ExecutiveSummaryParagraphs = summarySection is null || LooksLikeHtml(summarySection.ParagraphsText)
                ? fallback.ExecutiveSummaryParagraphs
                : ParseJoinedItems(summarySection.ParagraphsText),
            ExecutiveHighlights = item.HighlightEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Text)
                .ToList(),
            KeyFindings = item.FindingEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationFindingCardViewModel
                {
                    Icon = x.Icon,
                    Title = x.Title,
                    Description = x.Description
                })
                .ToList(),
            Sections = item.Sections
                .Where(x => x.SectionKey != "executive-summary")
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationArticleSectionViewModel
                {
                    Id = x.SectionKey,
                    Heading = x.Heading,
                    Paragraphs = ParseJoinedItems(x.ParagraphsText),
                    BulletPoints = ParseJoinedItems(x.BulletPointsText),
                    Callout = string.IsNullOrWhiteSpace(x.CalloutTone) || string.IsNullOrWhiteSpace(x.CalloutLabel) || string.IsNullOrWhiteSpace(x.CalloutTitle)
                        ? null
                        : new PublicationCalloutViewModel
                        {
                            Tone = x.CalloutTone,
                            Label = x.CalloutLabel,
                            Title = x.CalloutTitle,
                            Description = x.CalloutDescription ?? string.Empty
                        }
                })
                .ToList(),
            Charts = fallback.Charts,
            TableRows = fallback.TableRows,
            MapRoutes = fallback.MapRoutes,
            GalleryItems = ParseGalleryItems(item.GalleryJson).Count == 0 ? fallback.GalleryItems : ParseGalleryItems(item.GalleryJson),
            Documents = item.DocumentEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationDocumentCardViewModel
                {
                    Title = x.Title,
                    Type = x.Type,
                    Meta = x.Meta,
                    DownloadUrl = x.DownloadUrl ?? "#"
                })
                .ToList(),
            TableOfContents = item.Sections
                .Where(x => x.SectionKey != "executive-summary")
                .OrderBy(x => x.DisplayOrder)
                .Select((x, index) => new PublicationSidebarSectionLinkViewModel
                {
                    Label = $"{index + 1}. {x.Heading}",
                    TargetId = x.SectionKey
                })
                .ToList(),
            KeyTopics =
            [
                new() { Label = item.Type, TargetId = "key-findings" },
                new() { Label = item.CountryLabel, TargetId = item.Sections.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.SectionKey != "executive-summary")?.SectionKey ?? "overview" }
            ],
            SidebarDocuments = item.DocumentEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationSidebarLinkItemViewModel
                {
                    Title = x.Title,
                    Meta = x.Meta,
                    Url = x.DownloadUrl ?? "#"
                })
                .ToList(),
            RelatedIntelligence = item.RelatedLinks
                .Where(x => x.LinkType == "RelatedIntelligence")
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationSidebarLinkItemViewModel
                {
                    Title = x.Title,
                    Meta = x.Meta,
                    Url = x.Url ?? "#"
                })
                .ToList(),
            PublicationInfo = item.InfoEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationInfoRowViewModel
                {
                    Label = x.Label,
                    Value = x.Value
                })
                .ToList(),
            RelatedPublications = fallback.RelatedPublications
        };
    }

    private static IReadOnlyList<string> ParseJoinedItems(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split("~~", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    private static PublicationDetailsViewModel MergeWithFallback(
        PublicationDetailsViewModel details,
        PublicationDetailsViewModel fallback,
        string? rawJson = null)
    {
        var hasExplicitCharts = HasExplicitJsonCollection(rawJson, nameof(PublicationDetailsViewModel.Charts));
        var hasExplicitTableRows = HasExplicitJsonCollection(rawJson, nameof(PublicationDetailsViewModel.TableRows));

        return new PublicationDetailsViewModel
        {
            Title = string.IsNullOrWhiteSpace(details.Title) ? fallback.Title : details.Title,
            Slug = string.IsNullOrWhiteSpace(details.Slug) ? fallback.Slug : details.Slug,
            PublicationType = string.IsNullOrWhiteSpace(details.PublicationType) ? fallback.PublicationType : details.PublicationType,
            Topic = string.IsNullOrWhiteSpace(details.Topic) ? fallback.Topic : details.Topic,
            CoverageLabel = string.IsNullOrWhiteSpace(details.CoverageLabel) ? fallback.CoverageLabel : details.CoverageLabel,
            ShortDescription = string.IsNullOrWhiteSpace(details.ShortDescription) ? fallback.ShortDescription : details.ShortDescription,
            PublishedDate = string.IsNullOrWhiteSpace(details.PublishedDate) ? fallback.PublishedDate : details.PublishedDate,
            ReadingTime = string.IsNullOrWhiteSpace(details.ReadingTime) ? fallback.ReadingTime : details.ReadingTime,
            AuthorLabel = string.IsNullOrWhiteSpace(details.AuthorLabel) ? fallback.AuthorLabel : details.AuthorLabel,
            DocumentLabel = string.IsNullOrWhiteSpace(details.DocumentLabel) ? fallback.DocumentLabel : details.DocumentLabel,
            HeroVisualClassName = string.IsNullOrWhiteSpace(details.HeroVisualClassName) ? fallback.HeroVisualClassName : details.HeroVisualClassName,
            HeroImageUrl = string.IsNullOrWhiteSpace(details.HeroImageUrl) ? fallback.HeroImageUrl : details.HeroImageUrl,
            SourceName = string.IsNullOrWhiteSpace(details.SourceName) ? fallback.SourceName : details.SourceName,
            SourceUrl = string.IsNullOrWhiteSpace(details.SourceUrl) ? fallback.SourceUrl : details.SourceUrl,
            CountryLabels = details.CountryLabels.Count == 0 ? fallback.CountryLabels : details.CountryLabels,
            TopicLabels = details.TopicLabels.Count == 0 ? fallback.TopicLabels : details.TopicLabels,
            PdfDownloadUrl = string.IsNullOrWhiteSpace(details.PdfDownloadUrl) ? fallback.PdfDownloadUrl : details.PdfDownloadUrl,
            ExecutiveSummaryHtml = string.IsNullOrWhiteSpace(details.ExecutiveSummaryHtml) ? fallback.ExecutiveSummaryHtml : SanitizeRichTextHtml(details.ExecutiveSummaryHtml),
            IsPremium = details.IsPremium || fallback.IsPremium,
            Actions = details.Actions.Count == 0 ? fallback.Actions : details.Actions,
            Tabs = details.Tabs.Count == 0 ? fallback.Tabs : details.Tabs,
            Kpis = details.Kpis.Count == 0 ? fallback.Kpis : details.Kpis,
            ExecutiveSummaryParagraphs = details.ExecutiveSummaryParagraphs.Count == 0 ? fallback.ExecutiveSummaryParagraphs : details.ExecutiveSummaryParagraphs,
            ExecutiveHighlights = details.ExecutiveHighlights.Count == 0 ? fallback.ExecutiveHighlights : details.ExecutiveHighlights,
            KeyFindings = details.KeyFindings.Count == 0 ? fallback.KeyFindings : details.KeyFindings,
            Sections = details.Sections.Count == 0 ? fallback.Sections : details.Sections,
            Charts = details.Charts.Count == 0 && !hasExplicitCharts ? fallback.Charts : details.Charts,
            TableRows = details.TableRows.Count == 0 && !hasExplicitTableRows ? fallback.TableRows : details.TableRows,
            MapRoutes = details.MapRoutes.Count == 0 ? fallback.MapRoutes : details.MapRoutes,
            GalleryItems = details.GalleryItems.Count == 0 ? fallback.GalleryItems : details.GalleryItems,
            Documents = details.Documents.Count == 0 ? fallback.Documents : details.Documents,
            TableOfContents = details.TableOfContents.Count == 0 ? fallback.TableOfContents : details.TableOfContents,
            KeyTopics = details.KeyTopics.Count == 0 ? fallback.KeyTopics : details.KeyTopics,
            SidebarDocuments = details.SidebarDocuments.Count == 0 ? fallback.SidebarDocuments : details.SidebarDocuments,
            RelatedIntelligence = details.RelatedIntelligence.Count == 0 ? fallback.RelatedIntelligence : details.RelatedIntelligence,
            PublicationInfo = details.PublicationInfo.Count == 0 ? fallback.PublicationInfo : details.PublicationInfo,
            RelatedPublications = details.RelatedPublications.Count == 0 ? fallback.RelatedPublications : details.RelatedPublications
        };
    }

    private static PublicationDetailsViewModel BuildFallbackDetails(
        Domain.Content.PublicationContentItem item,
        IReadOnlyList<PublicationCardViewModel> relatedCards)
    {
        return new PublicationDetailsViewModel
        {
            Title = item.Title,
            Slug = item.Slug,
            PublicationType = item.Type,
            Topic = string.IsNullOrWhiteSpace(item.Topic) ? item.Type : item.Topic,
            CoverageLabel = item.CountryLabel,
            ShortDescription = item.Description,
            PublishedDate = string.IsNullOrWhiteSpace(item.PublishedDate) ? item.Meta.Split('|')[0].Trim() : item.PublishedDate,
            ReadingTime = string.IsNullOrWhiteSpace(item.ReadingTime) ? (item.Meta.Contains('|', StringComparison.Ordinal) ? item.Meta.Split('|')[1].Trim() : "12 min read") : item.ReadingTime,
            AuthorLabel = string.IsNullOrWhiteSpace(item.AuthorLabel) ? "CAFRI Research Team" : item.AuthorLabel,
            DocumentLabel = string.IsNullOrWhiteSpace(item.DocumentLabel) ? "PDF available | 1.2 MB" : item.DocumentLabel,
            HeroVisualClassName = string.IsNullOrWhiteSpace(item.HeroVisualClassName) ? "publication-hero-media--trade-corridors" : item.HeroVisualClassName,
            HeroImageUrl = item.HeroImageUrl ?? string.Empty,
            SourceName = item.SourceName ?? string.Empty,
            SourceUrl = item.SourceUrl ?? string.Empty,
            CountryLabels = BuildCountryLabels(item),
            TopicLabels = BuildTopicLabels(item),
            PdfDownloadUrl = item.PdfDownloadUrl ?? "#",
            IsPremium = true,
            ExecutiveSummaryHtml = string.Empty,
            Actions = DefaultActions.ToList(),
            Tabs = [ new() { Label = "Overview", TargetId = "overview", IsActive = true }, new() { Label = "Key Findings", TargetId = "key-findings" }, new() { Label = "Data & Charts", TargetId = "data-charts" }, new() { Label = "Documents", TargetId = "related-documents" }, new() { Label = "Related Publications", TargetId = "related-publications" } ],
            Kpis = [ new() { Label = "Coverage", Value = item.CountryLabel, Detail = "Primary focus", Tone = "accent" }, new() { Label = "Publication Type", Value = item.Type, Detail = "Research format", Tone = "neutral" }, new() { Label = "Reading Time", Value = string.IsNullOrWhiteSpace(item.ReadingTime) ? (item.Meta.Contains('|', StringComparison.Ordinal) ? item.Meta.Split('|')[1].Trim() : "12 min") : item.ReadingTime, Detail = "Estimated duration", Tone = "neutral" }, new() { Label = "Update Cycle", Value = "Monthly", Detail = "Monitoring cadence", Tone = "positive" }, new() { Label = "Access Level", Value = "Public", Detail = "Web article available", Tone = "accent" } ],
            ExecutiveSummaryParagraphs = [ $"{item.Title} provides a structured analytical overview of current developments relevant to {item.CountryLabel}. The page is designed as an online publication rather than a simple download screen, with summary findings, embedded data blocks, and related materials presented directly on the platform.", "This fallback detail layout is now generated from database content, so the page remains usable even before a full custom detail JSON has been authored in the admin panel." ],
            ExecutiveHighlights = [ "Core summary available directly on the page", "Structured sections ready for expanded editorial content", "Data, documents, and related materials grouped in one analytical workflow" ],
            KeyFindings = [ new() { Icon = "01", Title = "Structured publication format", Description = "The material is displayed as a navigable article page with sections, data, and supporting documents." }, new() { Icon = "02", Title = "Country and topic context", Description = $"The current report is positioned within CAFRI coverage for {item.CountryLabel} and related thematic monitoring." }, new() { Icon = "03", Title = "Admin-editable detail layer", Description = "Editors can later replace this generated fallback with a fully authored detail record from the admin area." } ],
            Sections = [ new() { Id = "regional-context", Heading = "Regional Context", Paragraphs = [$"This publication sits within CAFRI's broader analytical coverage of {item.CountryLabel} and Central Asian financial and regulatory developments.", "The article template supports incremental enrichment with report-specific findings, source references, and visual evidence without changing the overall page architecture."] }, new() { Id = "policy-reforms", Heading = "Analytical Context", Paragraphs = ["Each publication detail page is intended to hold a durable analytical narrative rather than only a downloadable document.", "That means users can access key findings, supporting data, and connected materials directly in the interface before deciding whether to download a PDF version."] } ],
            Charts = [ new() { Title = "Monitoring Snapshot", Subtitle = "Illustrative publication trend view", ChartType = "line", Labels = ["Q1", "Q2", "Q3", "Q4"], Series = [ new() { Label = "Signal Intensity", Color = "#1476ff", Values = [52m, 58m, 61m, 67m] } ] }, new() { Title = "Topic Allocation", Subtitle = "Illustrative thematic distribution", ChartType = "doughnut", ValueSuffix = "%", Labels = ["Policy", "Market", "Institutions", "Risk"], Series = [ new() { Label = "Share", Color = "#1476ff", Values = [30m, 24m, 21m, 25m] } ] } ],
            TableRows = [ new() { Corridor = "Primary Focus", CountriesCovered = item.CountryLabel, MainRoute = "Publication-specific", Volume = "-", Growth = "-", Status = "Active" }, new() { Corridor = "Regional Comparison", CountriesCovered = "Central Asia", MainRoute = "Comparative view", Volume = "-", Growth = "-", Status = "Monitored" } ],
            MapRoutes = [ new() { Name = "Coverage Lens", Countries = item.CountryLabel, TransitVolume = "N/A", AverageTime = "N/A", Status = "Monitoring", AccentColor = "#1476ff" } ],
            GalleryItems = ParseGalleryItems(item.GalleryJson),
            Documents = [ new() { Title = $"{item.Title} - Full PDF", Type = item.Type, Meta = $"{item.Meta} | PDF", DownloadUrl = "#" } ],
            TableOfContents = [ new() { Label = "1. Executive Summary", TargetId = "overview" }, new() { Label = "2. Regional Context", TargetId = "regional-context" }, new() { Label = "3. Analytical Context", TargetId = "policy-reforms" }, new() { Label = "4. Data & Charts", TargetId = "data-charts" } ],
            KeyTopics = [ new() { Label = item.Type, TargetId = "key-findings" }, new() { Label = item.CountryLabel, TargetId = "regional-context" } ],
            SidebarDocuments = [ new() { Title = $"{item.Title} - Full PDF", Meta = $"{item.Meta} | PDF" } ],
            RelatedIntelligence = [ new() { Title = $"{item.CountryLabel} monitoring overview", Meta = "Related intelligence material", Url = "/intelligence" } ],
            PublicationInfo = [ new() { Label = "Publication Type", Value = item.Type }, new() { Label = "Coverage", Value = item.CountryLabel }, new() { Label = "Published", Value = string.IsNullOrWhiteSpace(item.PublishedDate) ? item.Meta.Split('|')[0].Trim() : item.PublishedDate }, new() { Label = "Author", Value = string.IsNullOrWhiteSpace(item.AuthorLabel) ? "CAFRI Research Team" : item.AuthorLabel }, new() { Label = "Access", Value = "Public" } ],
            RelatedPublications = relatedCards
        };
    }
}
