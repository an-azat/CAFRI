using System.Text.Json;
using CAFRI.Areas.Admin.ViewModels.Content;
using CAFRI.Domain.Content;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Publications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class PublicationsController : Controller
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;
    private readonly ContentMediaStorageService _mediaStorage;

    public PublicationsController(AppDbContext dbContext, ContentMediaStorageService mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    private static IFormFile? ResolveSingleFile(IFormFile? modelFile, IFormFile? requestFile) =>
        modelFile is not null && modelFile.Length > 0 ? modelFile :
        requestFile is not null && requestFile.Length > 0 ? requestFile :
        null;

    private static IReadOnlyList<IFormFile> ResolveFiles(IFormFile[]? modelFiles, IReadOnlyList<IFormFile>? requestFiles)
    {
        var files = (modelFiles ?? Array.Empty<IFormFile>())
            .Where(file => file is not null && file.Length > 0)
            .ToList();

        if (files.Count != 0)
        {
            return files;
        }

        return (requestFiles ?? Array.Empty<IFormFile>())
            .Where(file => file is not null && file.Length > 0)
            .ToList();
    }

    private List<AdminCountryOptionViewModel> GetAvailableCountries()
    {
        var countries = _dbContext.CountryContents
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new AdminCountryOptionViewModel
            {
                Code = x.Code,
                Label = x.Name
            })
            .ToList();

        countries.Insert(0, new AdminCountryOptionViewModel
        {
            Code = "RG",
            Label = "Regional"
        });

        return countries;
    }

    private string ResolveCountryLabel(string countryCode, string fallbackLabel)
    {
        if (string.Equals(countryCode, "RG", StringComparison.OrdinalIgnoreCase))
        {
            return "Regional";
        }

        var normalizedCode = countryCode.Trim().ToUpperInvariant();
        var label = _dbContext.CountryContents
            .AsNoTracking()
            .Where(x => x.Code == normalizedCode)
            .Select(x => x.Name)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(label) ? fallbackLabel.Trim() : label;
    }

    private void ValidatePublicationModel(PublicationContentEditViewModel model, Guid? currentId = null)
    {
        if (!PublicationTopicMapper.CanonicalTypes.Contains(model.Type))
        {
            ModelState.AddModelError(nameof(model.Type), "Select a valid publication type.");
        }

        if (!PublicationTopicMapper.CanonicalTopics.Contains(model.Topic))
        {
            ModelState.AddModelError(nameof(model.Topic), "Select a valid publication topic.");
        }

        var allowedCountryCodes = GetAvailableCountries()
            .Select(x => x.Code)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (!allowedCountryCodes.Contains(model.CountryCode))
        {
            ModelState.AddModelError(nameof(model.CountryCode), "Select a valid country or regional coverage.");
        }

        if (!PublicationMetadataFormatter.CanonicalReadingTimes.Contains(model.ReadingTime))
        {
            ModelState.AddModelError(nameof(model.ReadingTime), "Select a valid reading time.");
        }

        if (!string.IsNullOrWhiteSpace(model.PublishedMonth) &&
            !DateTime.TryParseExact(model.PublishedMonth.Trim(), "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _))
        {
            ModelState.AddModelError(nameof(model.PublishedMonth), "Select a valid publication month.");
        }

        var normalizedSlug = model.Slug.Trim();
        var slugExists = _dbContext.PublicationContentItems
            .AsNoTracking()
            .Any(x => x.Slug == normalizedSlug && (!currentId.HasValue || x.Id != currentId.Value));

        if (slugExists)
        {
            ModelState.AddModelError(nameof(model.Slug), "A publication with this slug already exists.");
        }

        var heroFile = ResolveSingleFile(model.HeroImageFile, Request.Form.Files.GetFile(nameof(model.HeroImageFile)));
        if (heroFile is not null && !_mediaStorage.IsSupportedImage(heroFile))
        {
            ModelState.AddModelError(nameof(model.HeroImageFile), $"Unsupported hero image format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }

        var galleryFiles = ResolveFiles(model.GalleryFiles, Request.Form.Files.GetFiles(nameof(model.GalleryFiles)));
        if (galleryFiles.Any(file => !_mediaStorage.IsSupportedImage(file)))
        {
            ModelState.AddModelError(nameof(model.GalleryFiles), $"One or more gallery files use an unsupported format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? type, string? country, string? topic, string? status)
    {
        ViewData["Title"] = "Admin Publications";
        ViewData["AdminNav"] = "publications";

        var query = _dbContext.PublicationContentItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{term}%") ||
                EF.Functions.ILike(x.Description, $"%{term}%") ||
                EF.Functions.ILike(x.Type, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.Type == type);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(x => x.CountryLabel == country);
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            query = query.Where(x => x.Topic == topic);
        }

        if (status == "published")
        {
            query = query.Where(x => x.IsPublished);
        }
        else if (status == "draft")
        {
            query = query.Where(x => !x.IsPublished);
        }

        var items = await query
            .OrderBy(x => x.DisplayOrder)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ToListAsync();

        var allItems = await _dbContext.PublicationContentItems.AsNoTracking().ToListAsync();
        var monthStart = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        return View(new PublicationIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Publications", Value = allItems.Count.ToString(), Caption = "Database records" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.IsPublished).ToString(), Caption = "Visible on the platform" },
                new AdminSummaryCardViewModel { Label = "Drafts", Value = allItems.Count(x => !x.IsPublished).ToString(), Caption = "Hidden from public pages" },
                new AdminSummaryCardViewModel { Label = "Updated This Month", Value = allItems.Count(x => x.UpdatedAtUtc >= monthStart).ToString(), Caption = "Recently touched items" }
            ],
            AvailableTypes = allItems.Select(x => x.Type).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Order().ToList(),
            AvailableCountries = allItems.Select(x => x.CountryLabel).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Order().ToList(),
            AvailableTopics = allItems.Select(x => x.Topic).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Order().ToList(),
            Search = search,
            Type = type,
            Country = country,
            Topic = topic,
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Publication";
        ViewData["AdminNav"] = "publications";
        return View("Edit", BuildEditViewModel(new PublicationContentEditViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PublicationContentEditViewModel model)
    {
        ValidatePublicationModel(model);
        if (!ModelState.IsValid) return View("Edit", BuildEditViewModel(model));

        var normalizedPublishedDate = PublicationMetadataFormatter.NormalizePublishedDate(model.PublishedMonth, model.PublishedDate, model.Meta);
        var normalizedReadingTime = PublicationMetadataFormatter.NormalizeReadingTime(model.ReadingTime, model.Meta);
        var uploadedHeroFile = ResolveSingleFile(model.HeroImageFile, Request.Form.Files.GetFile(nameof(model.HeroImageFile)));
        var heroImageUrl = string.IsNullOrWhiteSpace(model.HeroImageUrl)
            ? await _mediaStorage.SaveImageAsync(uploadedHeroFile, "publications", HttpContext.RequestAborted)
            : model.HeroImageUrl.Trim();
        var galleryItems = ContentImageTextSerializer.Parse(model.GalleryText).ToList();
        galleryItems.AddRange(await _mediaStorage.SaveImagesAsync(ResolveFiles(model.GalleryFiles, Request.Form.Files.GetFiles(nameof(model.GalleryFiles))), "publications", HttpContext.RequestAborted));

        var entity = new PublicationContentItem
        {
            Id = Guid.NewGuid(),
            Slug = model.Slug.Trim(),
            Type = model.Type.Trim(),
            CountryCode = model.CountryCode.Trim(),
            CountryLabel = ResolveCountryLabel(model.CountryCode, model.CountryLabel),
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Meta = PublicationMetadataFormatter.BuildMeta(normalizedPublishedDate, normalizedReadingTime),
            Topic = PublicationTopicMapper.Normalize(model.Topic, model.Type, model.Title, model.Description),
            PublishedDate = normalizedPublishedDate,
            ReadingTime = normalizedReadingTime,
            AuthorLabel = string.IsNullOrWhiteSpace(model.AuthorLabel) ? "CAFRI Research Team" : model.AuthorLabel.Trim(),
            DocumentLabel = string.IsNullOrWhiteSpace(model.DocumentLabel) ? "PDF available | 1.2 MB" : model.DocumentLabel.Trim(),
            HeroVisualClassName = string.IsNullOrWhiteSpace(model.HeroVisualClassName) ? "publication-hero-media--trade-corridors" : model.HeroVisualClassName.Trim(),
            HeroImageUrl = heroImageUrl,
            GalleryJson = ContentImageTextSerializer.Serialize(galleryItems),
            PdfDownloadUrl = string.IsNullOrWhiteSpace(model.PdfDownloadUrl) ? "#" : model.PdfDownloadUrl.Trim(),
            DetailsJson = string.IsNullOrWhiteSpace(model.DetailsJson) ? null : model.DetailsJson.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _dbContext.PublicationContentItems.Add(entity);
        await ReplaceStructuredDetailsAsync(entity.Id, model);
        await _dbContext.SaveChangesAsync();
        TempData["AdminSuccess"] = $"Publication saved. Hero: {(string.IsNullOrWhiteSpace(entity.HeroImageUrl) ? "not saved" : entity.HeroImageUrl)}. Gallery items: {galleryItems.Count}.";
        return RedirectToAction(nameof(Edit), new { id = entity.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.PublicationContentItems
            .Include(x => x.ActionEntries)
            .Include(x => x.HighlightEntries)
            .Include(x => x.FindingEntries)
            .Include(x => x.Sections)
            .Include(x => x.DocumentEntries)
            .Include(x => x.RelatedLinks)
            .Include(x => x.InfoEntries)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (entity is null) return NotFound();

        ViewData["Title"] = "Edit Publication";
        ViewData["AdminNav"] = "publications";
        return View(BuildEditViewModel(entity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, PublicationContentEditViewModel model)
    {
        ValidatePublicationModel(model, id);
        if (!ModelState.IsValid) return View(BuildEditViewModel(model));

        var entity = await _dbContext.PublicationContentItems.FindAsync(id);
        if (entity is null) return NotFound();

        var normalizedPublishedDate = PublicationMetadataFormatter.NormalizePublishedDate(model.PublishedMonth, model.PublishedDate, model.Meta);
        var normalizedReadingTime = PublicationMetadataFormatter.NormalizeReadingTime(model.ReadingTime, model.Meta);
        var previousUrls = GetPublicationMediaUrls(entity).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var uploadedHeroImageUrl = await _mediaStorage.SaveImageAsync(ResolveSingleFile(model.HeroImageFile, Request.Form.Files.GetFile(nameof(model.HeroImageFile))), "publications", HttpContext.RequestAborted);
        var galleryItems = ContentImageTextSerializer.Parse(model.GalleryText).ToList();
        galleryItems.AddRange(await _mediaStorage.SaveImagesAsync(ResolveFiles(model.GalleryFiles, Request.Form.Files.GetFiles(nameof(model.GalleryFiles))), "publications", HttpContext.RequestAborted));

        entity.Slug = model.Slug.Trim();
        entity.Type = model.Type.Trim();
        entity.CountryCode = model.CountryCode.Trim();
        entity.CountryLabel = ResolveCountryLabel(model.CountryCode, model.CountryLabel);
        entity.Title = model.Title.Trim();
        entity.Description = model.Description.Trim();
        entity.Meta = PublicationMetadataFormatter.BuildMeta(normalizedPublishedDate, normalizedReadingTime);
        entity.Topic = PublicationTopicMapper.Normalize(model.Topic, model.Type, model.Title, model.Description);
        entity.PublishedDate = normalizedPublishedDate;
        entity.ReadingTime = normalizedReadingTime;
        entity.AuthorLabel = string.IsNullOrWhiteSpace(model.AuthorLabel) ? entity.AuthorLabel : model.AuthorLabel.Trim();
        entity.DocumentLabel = string.IsNullOrWhiteSpace(model.DocumentLabel) ? entity.DocumentLabel : model.DocumentLabel.Trim();
        entity.HeroVisualClassName = string.IsNullOrWhiteSpace(model.HeroVisualClassName) ? entity.HeroVisualClassName : model.HeroVisualClassName.Trim();
        entity.HeroImageUrl = !string.IsNullOrWhiteSpace(uploadedHeroImageUrl)
            ? uploadedHeroImageUrl
            : string.IsNullOrWhiteSpace(model.HeroImageUrl) ? entity.HeroImageUrl : model.HeroImageUrl.Trim();
        entity.GalleryJson = ContentImageTextSerializer.Serialize(galleryItems);
        entity.PdfDownloadUrl = string.IsNullOrWhiteSpace(model.PdfDownloadUrl) ? entity.PdfDownloadUrl : model.PdfDownloadUrl.Trim();
        entity.DetailsJson = string.IsNullOrWhiteSpace(model.DetailsJson) ? entity.DetailsJson : model.DetailsJson.Trim();
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsPublished = model.IsPublished;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await ReplaceStructuredDetailsAsync(entity.Id, model);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(previousUrls.Except(GetPublicationMediaUrls(entity), StringComparer.OrdinalIgnoreCase));
        TempData["AdminSuccess"] = $"Publication updated. Hero: {(string.IsNullOrWhiteSpace(entity.HeroImageUrl) ? "not saved" : entity.HeroImageUrl)}. Gallery items: {galleryItems.Count}.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _dbContext.PublicationContentItems.FindAsync(id);
        if (entity is null) return NotFound();
        var deletedUrls = GetPublicationMediaUrls(entity).ToList();
        _dbContext.PublicationContentItems.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(deletedUrls);
        return RedirectToAction(nameof(Index));
    }

    private static IEnumerable<string> GetPublicationMediaUrls(PublicationContentItem entity)
    {
        if (!string.IsNullOrWhiteSpace(entity.HeroImageUrl))
        {
            yield return entity.HeroImageUrl;
        }

        foreach (var item in ContentImageTextSerializer.Parse(entity.GalleryJson))
        {
            if (!string.IsNullOrWhiteSpace(item.ImageUrl))
            {
                yield return item.ImageUrl;
            }
        }
    }

    private async Task DeleteUnusedMediaAsync(IEnumerable<string> urls)
    {
        foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var stillUsed = await _dbContext.CountryContents.AsNoTracking().AnyAsync(x => x.HeroGalleryJson != null && x.HeroGalleryJson.Contains(url)) ||
                            await _dbContext.PublicationContentItems.AsNoTracking().AnyAsync(x =>
                                x.HeroImageUrl == url ||
                                (x.GalleryJson != null && x.GalleryJson.Contains(url))) ||
                            await _dbContext.IntelligenceContentItems.AsNoTracking().AnyAsync(x => x.HeroImageUrl == url);

            if (!stillUsed)
            {
                _mediaStorage.DeleteImage(url);
            }
        }
    }

    private PublicationContentEditViewModel BuildEditViewModel(PublicationContentItem entity)
    {
        var details = HasStructuredDetails(entity)
            ? BuildStructuredDetails(entity, BuildDefaultDetails(entity))
            : DeserializeDetails(entity.DetailsJson) ?? BuildDefaultDetails(entity);

        return new PublicationContentEditViewModel
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Type = entity.Type,
            AvailableTypes = PublicationTopicMapper.CanonicalTypes,
            CountryCode = entity.CountryCode,
            CountryLabel = entity.CountryLabel,
            AvailableCountries = GetAvailableCountries(),
            Title = entity.Title,
            Description = entity.Description,
            Meta = entity.Meta,
            DetailsJson = entity.DetailsJson,
            Topic = PublicationTopicMapper.Normalize(entity.Topic, entity.Type, entity.Title, entity.Description),
            AvailableTopics = PublicationTopicMapper.CanonicalTopics,
            PublishedMonth = PublicationMetadataFormatter.ToMonthInputValue(string.IsNullOrWhiteSpace(entity.PublishedDate) ? details.PublishedDate : entity.PublishedDate),
            PublishedDate = string.IsNullOrWhiteSpace(entity.PublishedDate) ? details.PublishedDate : entity.PublishedDate,
            ReadingTime = string.IsNullOrWhiteSpace(entity.ReadingTime) ? details.ReadingTime : entity.ReadingTime,
            AvailableReadingTimes = PublicationMetadataFormatter.CanonicalReadingTimes,
            AuthorLabel = string.IsNullOrWhiteSpace(entity.AuthorLabel) ? details.AuthorLabel : entity.AuthorLabel,
            DocumentLabel = string.IsNullOrWhiteSpace(entity.DocumentLabel) ? details.DocumentLabel : entity.DocumentLabel,
            HeroVisualClassName = string.IsNullOrWhiteSpace(entity.HeroVisualClassName) ? details.HeroVisualClassName : entity.HeroVisualClassName,
            HeroImageUrl = entity.HeroImageUrl ?? string.Empty,
            PdfDownloadUrl = string.IsNullOrWhiteSpace(entity.PdfDownloadUrl) ? details.PdfDownloadUrl : entity.PdfDownloadUrl,
            ActionsText = PublicationStructuredDetailParsers.JoinActions(details.Actions),
            ExecutiveSummaryText = PublicationStructuredDetailParsers.JoinParagraphs(details.ExecutiveSummaryParagraphs),
            ExecutiveHighlightsText = PublicationStructuredDetailParsers.JoinParagraphs(details.ExecutiveHighlights),
            KeyFindingsText = PublicationStructuredDetailParsers.JoinKeyFindings(details.KeyFindings),
            SectionsText = PublicationStructuredDetailParsers.JoinSections(details.Sections),
            GalleryText = ContentImageTextSerializer.Serialize(details.GalleryItems.Select(item => new CAFRI.ViewModels.Shared.ContentImageItemViewModel
            {
                ImageUrl = item.ImageUrl,
                Alt = item.Alt,
                Title = item.Title,
                Caption = item.Caption
            })),
            DocumentsText = PublicationStructuredDetailParsers.JoinDocuments(details.Documents),
            RelatedIntelligenceText = PublicationStructuredDetailParsers.JoinRelatedLinks(details.RelatedIntelligence),
            PublicationInfoText = PublicationStructuredDetailParsers.JoinInfo(details.PublicationInfo),
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished
        };
    }

    private PublicationContentEditViewModel BuildEditViewModel(PublicationContentEditViewModel model) =>
        new()
        {
            Id = model.Id,
            Slug = model.Slug,
            Type = model.Type,
            AvailableTypes = PublicationTopicMapper.CanonicalTypes,
            CountryCode = model.CountryCode,
            CountryLabel = ResolveCountryLabel(model.CountryCode, model.CountryLabel),
            AvailableCountries = GetAvailableCountries(),
            Title = model.Title,
            Description = model.Description,
            Meta = model.Meta,
            DetailsJson = model.DetailsJson,
            Topic = string.IsNullOrWhiteSpace(model.Topic)
                ? PublicationTopicMapper.Normalize(model.Topic, model.Type, model.Title, model.Description)
                : model.Topic,
            AvailableTopics = PublicationTopicMapper.CanonicalTopics,
            PublishedMonth = string.IsNullOrWhiteSpace(model.PublishedMonth)
                ? PublicationMetadataFormatter.ToMonthInputValue(model.PublishedDate)
                : model.PublishedMonth,
            PublishedDate = model.PublishedDate,
            ReadingTime = model.ReadingTime,
            AvailableReadingTimes = PublicationMetadataFormatter.CanonicalReadingTimes,
            AuthorLabel = model.AuthorLabel,
            DocumentLabel = model.DocumentLabel,
            HeroVisualClassName = model.HeroVisualClassName,
            HeroImageUrl = model.HeroImageUrl,
            PdfDownloadUrl = model.PdfDownloadUrl,
            ActionsText = model.ActionsText,
            ExecutiveSummaryText = model.ExecutiveSummaryText,
            ExecutiveHighlightsText = model.ExecutiveHighlightsText,
            KeyFindingsText = model.KeyFindingsText,
            SectionsText = model.SectionsText,
            GalleryText = model.GalleryText,
            DocumentsText = model.DocumentsText,
            RelatedIntelligenceText = model.RelatedIntelligenceText,
            PublicationInfoText = model.PublicationInfoText,
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished
        };

    private static bool HasStructuredDetails(PublicationContentItem entity) =>
        entity.ActionEntries.Count != 0 ||
        entity.HighlightEntries.Count != 0 ||
        entity.FindingEntries.Count != 0 ||
        entity.Sections.Count != 0 ||
        entity.DocumentEntries.Count != 0 ||
        entity.RelatedLinks.Count != 0 ||
        entity.InfoEntries.Count != 0;

    private static PublicationDetailsViewModel? DeserializeDetails(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<PublicationDetailsViewModel>(detailsJson, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private static PublicationDetailsViewModel BuildStructuredDetails(
        PublicationContentItem entity,
        PublicationDetailsViewModel fallback)
    {
        var summarySection = entity.Sections
            .Where(x => x.SectionKey == "executive-summary")
            .OrderBy(x => x.DisplayOrder)
            .FirstOrDefault();

        var details = new PublicationDetailsViewModel
        {
            Title = entity.Title,
            Slug = entity.Slug,
            PublicationType = entity.Type,
            Topic = entity.Topic,
            CoverageLabel = entity.CountryLabel,
            ShortDescription = entity.Description,
            PublishedDate = entity.PublishedDate,
            ReadingTime = entity.ReadingTime,
            AuthorLabel = entity.AuthorLabel,
            DocumentLabel = entity.DocumentLabel,
            HeroVisualClassName = entity.HeroVisualClassName,
            PdfDownloadUrl = entity.PdfDownloadUrl ?? "#",
            Actions = entity.ActionEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationActionItemViewModel { Label = x.Label })
                .ToList(),
            Tabs = fallback.Tabs,
            Kpis = fallback.Kpis,
            ExecutiveSummaryParagraphs = summarySection is null
                ? fallback.ExecutiveSummaryParagraphs
                : PublicationStructuredDetailParsers.ParseMultiValueSegment(summarySection.ParagraphsText),
            ExecutiveHighlights = entity.HighlightEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.Text)
                .ToList(),
            KeyFindings = entity.FindingEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationFindingCardViewModel
                {
                    Icon = x.Icon,
                    Title = x.Title,
                    Description = x.Description
                })
                .ToList(),
            Sections = entity.Sections
                .Where(x => x.SectionKey != "executive-summary")
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationArticleSectionViewModel
                {
                    Id = x.SectionKey,
                    Heading = x.Heading,
                    Paragraphs = PublicationStructuredDetailParsers.ParseMultiValueSegment(x.ParagraphsText),
                    BulletPoints = PublicationStructuredDetailParsers.ParseMultiValueSegment(x.BulletPointsText),
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
            GalleryItems = fallback.GalleryItems,
            Documents = entity.DocumentEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationDocumentCardViewModel
                {
                    Title = x.Title,
                    Type = x.Type,
                    Meta = x.Meta,
                    DownloadUrl = x.DownloadUrl ?? "#"
                })
                .ToList(),
            TableOfContents = entity.Sections
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
                new() { Label = entity.Type, TargetId = "key-findings" },
                new() { Label = entity.CountryLabel, TargetId = entity.Sections.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.SectionKey != "executive-summary")?.SectionKey ?? "overview" }
            ],
            SidebarDocuments = entity.DocumentEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationSidebarLinkItemViewModel
                {
                    Title = x.Title,
                    Meta = x.Meta,
                    Url = x.DownloadUrl ?? "#"
                })
                .ToList(),
            RelatedIntelligence = entity.RelatedLinks
                .Where(x => x.LinkType == "RelatedIntelligence")
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationSidebarLinkItemViewModel
                {
                    Title = x.Title,
                    Meta = x.Meta,
                    Url = x.Url ?? "#"
                })
                .ToList(),
            PublicationInfo = entity.InfoEntries
                .OrderBy(x => x.DisplayOrder)
                .Select(x => new PublicationInfoRowViewModel
                {
                    Label = x.Label,
                    Value = x.Value
                })
                .ToList(),
            RelatedPublications = fallback.RelatedPublications
        };

        return details;
    }

    private static PublicationDetailsViewModel BuildDefaultDetails(PublicationContentItem item)
    {
        var publishedDate = string.IsNullOrWhiteSpace(item.PublishedDate) ? item.Meta.Split('|')[0].Trim() : item.PublishedDate;
        var readingTime = string.IsNullOrWhiteSpace(item.ReadingTime) ? PublicationMetadataFormatter.NormalizeReadingTime(null, item.Meta) : item.ReadingTime;

        return new PublicationDetailsViewModel
        {
            Title = item.Title,
            Slug = item.Slug,
            PublicationType = item.Type,
            Topic = string.IsNullOrWhiteSpace(item.Topic) ? item.Type : item.Topic,
            CoverageLabel = item.CountryLabel,
            ShortDescription = item.Description,
            PublishedDate = publishedDate,
            ReadingTime = readingTime,
            AuthorLabel = string.IsNullOrWhiteSpace(item.AuthorLabel) ? "CAFRI Research Team" : item.AuthorLabel,
            DocumentLabel = string.IsNullOrWhiteSpace(item.DocumentLabel) ? "PDF available | 1.2 MB" : item.DocumentLabel,
            HeroVisualClassName = string.IsNullOrWhiteSpace(item.HeroVisualClassName) ? "publication-hero-media--trade-corridors" : item.HeroVisualClassName,
            PdfDownloadUrl = item.PdfDownloadUrl ?? "#",
            Actions =
            [
                new() { Label = "Read Online" },
                new() { Label = "Download Full Report (PDF)" },
                new() { Label = "Share" },
                new() { Label = "Save" },
                new() { Label = "Cite" },
                new() { Label = "Print" }
            ],
            Tabs =
            [
                new() { Label = "Overview", TargetId = "overview", IsActive = true },
                new() { Label = "Key Findings", TargetId = "key-findings" },
                new() { Label = "Data & Charts", TargetId = "data-charts" },
                new() { Label = "Documents", TargetId = "related-documents" },
                new() { Label = "Related Publications", TargetId = "related-publications" }
            ],
            Kpis =
            [
                new() { Label = "Coverage", Value = item.CountryLabel, Detail = "Primary focus", Tone = "accent" },
                new() { Label = "Publication Type", Value = item.Type, Detail = "Research format", Tone = "neutral" },
                new() { Label = "Reading Time", Value = readingTime, Detail = "Estimated duration", Tone = "neutral" },
                new() { Label = "Update Cycle", Value = "Monthly", Detail = "Monitoring cadence", Tone = "positive" },
                new() { Label = "Access Level", Value = "Public", Detail = "Web article available", Tone = "accent" }
            ],
            ExecutiveSummaryParagraphs =
            [
                $"{item.Title} provides a structured analytical overview of current developments relevant to {item.CountryLabel}. The page is designed as an online publication rather than a simple download screen, with summary findings, embedded data blocks, and related materials presented directly on the platform.",
                "This fallback detail layout is now generated from database content, so the page remains usable even before a full custom detail JSON has been authored in the admin panel."
            ],
            ExecutiveHighlights =
            [
                "Core summary available directly on the page",
                "Structured sections ready for expanded editorial content",
                "Data, documents, and related materials grouped in one analytical workflow"
            ],
            KeyFindings =
            [
                new() { Icon = "01", Title = "Structured publication format", Description = "The material is displayed as a navigable article page with sections, data, and supporting documents." },
                new() { Icon = "02", Title = "Country and topic context", Description = $"The current report is positioned within CAFRI coverage for {item.CountryLabel} and related thematic monitoring." },
                new() { Icon = "03", Title = "Admin-editable detail layer", Description = "Editors can later replace this generated fallback with a fully authored detail record from the admin area." }
            ],
            Sections =
            [
                new() { Id = "regional-context", Heading = "Regional Context", Paragraphs = [$"This publication sits within CAFRI's broader analytical coverage of {item.CountryLabel} and Central Asian financial and regulatory developments.", "The article template supports incremental enrichment with report-specific findings, source references, and visual evidence without changing the overall page architecture."] },
                new() { Id = "policy-reforms", Heading = "Analytical Context", Paragraphs = ["Each publication detail page is intended to hold a durable analytical narrative rather than only a downloadable document.", "That means users can access key findings, supporting data, and connected materials directly in the interface before deciding whether to download a PDF version."] }
            ],
            Charts = [],
            TableRows = [],
            MapRoutes = [],
            GalleryItems = [],
            Documents = [ new() { Title = $"{item.Title} - Full PDF", Type = item.Type, Meta = $"{item.Meta} | PDF", DownloadUrl = "#" } ],
            TableOfContents =
            [
                new() { Label = "1. Executive Summary", TargetId = "overview" },
                new() { Label = "2. Regional Context", TargetId = "regional-context" },
                new() { Label = "3. Analytical Context", TargetId = "policy-reforms" },
                new() { Label = "4. Data & Charts", TargetId = "data-charts" }
            ],
            KeyTopics =
            [
                new() { Label = item.Type, TargetId = "key-findings" },
                new() { Label = item.CountryLabel, TargetId = "regional-context" }
            ],
            SidebarDocuments = [ new() { Title = $"{item.Title} - Full PDF", Meta = $"{item.Meta} | PDF" } ],
            RelatedIntelligence = [ new() { Title = $"{item.CountryLabel} monitoring overview", Meta = "Related intelligence material", Url = "/intelligence" } ],
            PublicationInfo =
            [
                new() { Label = "Publication Type", Value = item.Type },
                new() { Label = "Coverage", Value = item.CountryLabel },
                new() { Label = "Published", Value = publishedDate },
                new() { Label = "Author", Value = "CAFRI Research Team" },
                new() { Label = "Access", Value = "Public" }
            ],
            RelatedPublications = []
        };
    }

    private async Task ReplaceStructuredDetailsAsync(Guid publicationContentItemId, PublicationContentEditViewModel model)
    {
        var existingActions = await _dbContext.PublicationActionEntries.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();
        var existingHighlights = await _dbContext.PublicationHighlightEntries.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();
        var existingFindings = await _dbContext.PublicationFindingEntries.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();
        var existingSections = await _dbContext.PublicationArticleSections.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();
        var existingDocuments = await _dbContext.PublicationDocumentEntries.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();
        var existingLinks = await _dbContext.PublicationRelatedLinks.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();
        var existingInfo = await _dbContext.PublicationInfoEntries.Where(x => x.PublicationContentItemId == publicationContentItemId).ToListAsync();

        _dbContext.PublicationActionEntries.RemoveRange(existingActions);
        _dbContext.PublicationHighlightEntries.RemoveRange(existingHighlights);
        _dbContext.PublicationFindingEntries.RemoveRange(existingFindings);
        _dbContext.PublicationArticleSections.RemoveRange(existingSections);
        _dbContext.PublicationDocumentEntries.RemoveRange(existingDocuments);
        _dbContext.PublicationRelatedLinks.RemoveRange(existingLinks);
        _dbContext.PublicationInfoEntries.RemoveRange(existingInfo);

        _dbContext.PublicationActionEntries.AddRange(
            PublicationStructuredDetailParsers.ParseLineCollection(model.ActionsText)
                .Select((label, index) => new PublicationActionEntry
                {
                    Id = Guid.NewGuid(),
                    PublicationContentItemId = publicationContentItemId,
                    Label = label,
                    DisplayOrder = index
                }));

        var executiveSummary = PublicationStructuredDetailParsers.ParseLineCollection(model.ExecutiveSummaryText);
        if (executiveSummary.Count != 0)
        {
            _dbContext.PublicationArticleSections.Add(new PublicationArticleSection
            {
                Id = Guid.NewGuid(),
                PublicationContentItemId = publicationContentItemId,
                SectionKey = "executive-summary",
                Heading = "Executive Summary",
                ParagraphsText = string.Join("~~", executiveSummary),
                BulletPointsText = string.Empty,
                DisplayOrder = 0
            });
        }

        _dbContext.PublicationHighlightEntries.AddRange(
            PublicationStructuredDetailParsers.ParseLineCollection(model.ExecutiveHighlightsText)
                .Select((text, index) => new PublicationHighlightEntry
                {
                    Id = Guid.NewGuid(),
                    PublicationContentItemId = publicationContentItemId,
                    Text = text,
                    DisplayOrder = index
                }));

        var findingRows = PublicationStructuredDetailParsers.ParsePipeRows(model.KeyFindingsText, 3);
        _dbContext.PublicationFindingEntries.AddRange(findingRows.Select((parts, index) => new PublicationFindingEntry
        {
            Id = Guid.NewGuid(),
            PublicationContentItemId = publicationContentItemId,
            Icon = parts[0],
            Title = parts[1],
            Description = string.Join(" | ", parts.Skip(2)),
            DisplayOrder = index
        }));

        var sectionRows = PublicationStructuredDetailParsers.ParsePipeRows(model.SectionsText, 4);
        _dbContext.PublicationArticleSections.AddRange(sectionRows.Select((parts, index) => new PublicationArticleSection
        {
            Id = Guid.NewGuid(),
            PublicationContentItemId = publicationContentItemId,
            SectionKey = parts[0],
            Heading = parts[1],
            ParagraphsText = parts[2].Replace(" ~~ ", "~~", StringComparison.Ordinal),
            BulletPointsText = parts[3].Replace(" ~~ ", "~~", StringComparison.Ordinal),
            CalloutTone = parts.Length > 4 && !string.IsNullOrWhiteSpace(parts[4]) ? parts[4] : null,
            CalloutLabel = parts.Length > 5 && !string.IsNullOrWhiteSpace(parts[5]) ? parts[5] : null,
            CalloutTitle = parts.Length > 6 && !string.IsNullOrWhiteSpace(parts[6]) ? parts[6] : null,
            CalloutDescription = parts.Length > 7 && !string.IsNullOrWhiteSpace(parts[7]) ? string.Join(" | ", parts.Skip(7)) : null,
            DisplayOrder = index + 1
        }));

        var documentRows = PublicationStructuredDetailParsers.ParsePipeRows(model.DocumentsText, 3);
        _dbContext.PublicationDocumentEntries.AddRange(documentRows.Select((parts, index) => new PublicationDocumentEntry
        {
            Id = Guid.NewGuid(),
            PublicationContentItemId = publicationContentItemId,
            Title = parts[0],
            Type = parts[1],
            Meta = parts[2],
            DownloadUrl = parts.Length > 3 ? string.Join(" | ", parts.Skip(3)) : "#",
            DisplayOrder = index
        }));

        var relatedRows = PublicationStructuredDetailParsers.ParsePipeRows(model.RelatedIntelligenceText, 2);
        _dbContext.PublicationRelatedLinks.AddRange(relatedRows.Select((parts, index) => new PublicationRelatedLink
        {
            Id = Guid.NewGuid(),
            PublicationContentItemId = publicationContentItemId,
            LinkType = "RelatedIntelligence",
            Title = parts[0],
            Meta = parts[1],
            Url = parts.Length > 2 ? string.Join(" | ", parts.Skip(2)) : "#",
            DisplayOrder = index
        }));

        var infoRows = PublicationStructuredDetailParsers.ParsePipeRows(model.PublicationInfoText, 2);
        _dbContext.PublicationInfoEntries.AddRange(infoRows.Select((parts, index) => new PublicationInfoEntry
        {
            Id = Guid.NewGuid(),
            PublicationContentItemId = publicationContentItemId,
            Label = parts[0],
            Value = string.Join(" | ", parts.Skip(1)),
            DisplayOrder = index
        }));
    }

}
