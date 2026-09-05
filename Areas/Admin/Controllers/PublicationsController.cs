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
    private static readonly string[] DefaultPublicationActions = ["Download Full Report", "Share", "Print"];
    private sealed record UploadedPublicationDocumentFile(string Title, string Type, string Meta, string DownloadUrl);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly AppDbContext _dbContext;
    private readonly ContentMediaStorageService _mediaStorage;

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

    // Image/document uploads that fail the format checks in ContentMediaStorageService
    // (unsupported extension, oversized, or a magic-number mismatch for a renamed/corrupted
    // file) are silently skipped by design there - this turns that silence into a visible
    // admin-facing warning instead of a publication that quietly saved without its media.
    private static string? BuildFileUploadWarning(
        IFormFile? providedHeroFile,
        string? savedHeroUrl,
        int providedGalleryCount,
        int savedGalleryCount,
        int providedDocumentCount,
        int savedDocumentCount)
    {
        var issues = new List<string>();

        if (providedHeroFile is not null && providedHeroFile.Length > 0 && string.IsNullOrWhiteSpace(savedHeroUrl))
        {
            issues.Add($"hero image \"{providedHeroFile.FileName}\" could not be saved");
        }

        var droppedGallery = providedGalleryCount - savedGalleryCount;
        if (droppedGallery > 0)
        {
            issues.Add($"{droppedGallery} gallery photo(s) could not be saved");
        }

        var droppedDocuments = providedDocumentCount - savedDocumentCount;
        if (droppedDocuments > 0)
        {
            issues.Add($"{droppedDocuments} document(s) could not be saved");
        }

        return issues.Count == 0
            ? null
            : $"Some uploads were skipped ({string.Join("; ", issues)}). The file format could not be verified - try re-exporting/re-saving it and upload again.";
    }

    private static string FormatFileSize(long bytes)
    {
        if (bytes >= 1024 * 1024)
        {
            return $"{bytes / (1024d * 1024d):0.#} MB";
        }

        if (bytes >= 1024)
        {
            return $"{bytes / 1024d:0.#} KB";
        }

        return $"{bytes} B";
    }

    private static string BuildDocumentTypeFromExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).TrimStart('.').ToUpperInvariant();
        return string.IsNullOrWhiteSpace(extension) ? "FILE" : extension;
    }

    private async Task<IReadOnlyList<UploadedPublicationDocumentFile>> BuildUploadedPublicationDocumentsAsync(
        Guid publicationId,
        IReadOnlyList<IFormFile> files)
    {
        if (files.Count == 0)
        {
            return [];
        }

        var items = new List<UploadedPublicationDocumentFile>();
        foreach (var file in files)
        {
            var url = await _mediaStorage.SaveDocumentAsync(file, "publications", HttpContext.RequestAborted);
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var title = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace('_', ' ')
                .Replace('-', ' ')
                .Trim();

            items.Add(new UploadedPublicationDocumentFile(
                string.IsNullOrWhiteSpace(title) ? "Publication document" : title,
                BuildDocumentTypeFromExtension(file.FileName),
                $"{BuildDocumentTypeFromExtension(file.FileName)} | {FormatFileSize(file.Length)}",
                url));
        }

        return items;
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

    private static bool TryParseDateInput(string? value, out DateTime date)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            date = default;
            return false;
        }

        return DateTime.TryParseExact(
            value.Trim(),
            "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out date);
    }

    private async Task ReplaceAssignmentsAsync(Guid publicationId, string? countriesText, string? categoriesText)
    {
        var existingCountries = await _dbContext.PublicationCountryAssignments
            .Where(x => x.PublicationContentItemId == publicationId)
            .ToListAsync();
        var existingCategories = await _dbContext.PublicationCategoryAssignments
            .Where(x => x.PublicationContentItemId == publicationId)
            .ToListAsync();

        _dbContext.PublicationCountryAssignments.RemoveRange(existingCountries);
        _dbContext.PublicationCategoryAssignments.RemoveRange(existingCategories);

        var countries = await PublicationImportHelpers.ResolveCountryAssignmentsAsync(
            _dbContext,
            PublicationImportHelpers.ParseDelimitedValues(countriesText),
            publicationId,
            HttpContext.RequestAborted);
        var categories = await PublicationImportHelpers.ResolveCategoryAssignmentsAsync(
            _dbContext,
            PublicationImportHelpers.ParseDelimitedValues(categoriesText),
            publicationId,
            HttpContext.RequestAborted);

        _dbContext.PublicationCountryAssignments.AddRange(countries);
        _dbContext.PublicationCategoryAssignments.AddRange(categories);
    }

    private void ValidatePublicationModel(PublicationContentEditViewModel model, Guid? currentId = null)
    {
        if (!PublicationWorkflowStatuses.IsValid(model.WorkflowStatus))
        {
            ModelState.AddModelError(nameof(model.WorkflowStatus), "Select a valid publication status.");
        }

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

        if (!string.IsNullOrWhiteSpace(model.SourcePublishedAtUtc) &&
            !DateTimeOffset.TryParse(model.SourcePublishedAtUtc.Trim(), out _))
        {
            ModelState.AddModelError(nameof(model.SourcePublishedAtUtc), "Enter a valid source publication date.");
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

        var documentFiles = ResolveFiles(model.DocumentFiles, Request.Form.Files.GetFiles(nameof(model.DocumentFiles)));
        if (documentFiles.Any(file => !_mediaStorage.IsSupportedDocument(file)))
        {
            ModelState.AddModelError(nameof(model.DocumentFiles), $"One or more document files use an unsupported format. Allowed: {_mediaStorage.GetAllowedDocumentExtensionsLabel()}");
        }
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? type, string? country, string? topic, string? source, string? status, string? dateFrom, string? dateTo)
    {
        ViewData["Title"] = "Admin Publications";
        ViewData["AdminNav"] = "publications";

        var query = _dbContext.PublicationContentItems
            .AsNoTracking()
            .Include(x => x.CountryAssignments)
            .Include(x => x.CategoryAssignments)
            .AsQueryable();

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
            query = query.Where(x =>
                x.CountryLabel == country ||
                x.CountryAssignments.Any(item => item.CountryLabel == country || item.CountryCode == country));
        }

        if (!string.IsNullOrWhiteSpace(topic))
        {
            query = query.Where(x =>
                x.Topic == topic ||
                x.CategoryAssignments.Any(item => item.CategoryName == topic || item.CategorySlug == topic));
        }

        if (!string.IsNullOrWhiteSpace(source))
        {
            query = query.Where(x => x.SourceName == source || x.SourceDomain == source);
        }

        if (status == "published")
        {
            query = query.Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Published);
        }
        else if (status == "draft")
        {
            query = query.Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Draft);
        }
        else if (status == "rejected")
        {
            query = query.Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Rejected);
        }
        else if (status == "archived")
        {
            query = query.Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Archived);
        }
        else if (status == "review")
        {
            query = query.Where(x => x.RequiresReview);
        }

        if (TryParseDateInput(dateFrom, out var fromDate))
        {
            var start = new DateTimeOffset(fromDate, TimeSpan.Zero);
            query = query.Where(x => (x.SourcePublishedAtUtc ?? x.CreatedAtUtc) >= start);
        }

        if (TryParseDateInput(dateTo, out var toDate))
        {
            var endExclusive = new DateTimeOffset(toDate.AddDays(1), TimeSpan.Zero);
            query = query.Where(x => (x.SourcePublishedAtUtc ?? x.CreatedAtUtc) < endExclusive);
        }

        var items = await query
            .OrderBy(x => x.WorkflowStatus == PublicationWorkflowStatuses.Draft && x.ImportedAtUtc != null ? 0 : 1)
            .ThenByDescending(x => x.ImportedAtUtc ?? x.UpdatedAtUtc)
            .ThenBy(x => x.DisplayOrder)
            .ToListAsync();

        var allItems = await _dbContext.PublicationContentItems
            .AsNoTracking()
            .Include(x => x.CountryAssignments)
            .Include(x => x.CategoryAssignments)
            .ToListAsync();
        var monthStart = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var todayStart = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);
        var importedDraftCount = allItems.Count(x => x.ImportedAtUtc != null && x.WorkflowStatus == PublicationWorkflowStatuses.Draft);
        var reviewRequiredCount = allItems.Count(x => x.RequiresReview && x.WorkflowStatus == PublicationWorkflowStatuses.Draft);
        var importedTodayCount = allItems.Count(x => x.ImportedAtUtc >= todayStart);
        var reviewQueue = allItems
            .Where(x => x.WorkflowStatus == PublicationWorkflowStatuses.Draft && (x.RequiresReview || x.ImportedAtUtc != null))
            .OrderByDescending(x => x.RequiresReview)
            .ThenByDescending(x => x.ImportedAtUtc ?? x.UpdatedAtUtc)
            .Take(6)
            .ToList();

        return View(new PublicationIndexViewModel
        {
            Items = items,
            ReviewQueue = reviewQueue,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Publications", Value = allItems.Count.ToString(), Caption = "Database records" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.WorkflowStatus == PublicationWorkflowStatuses.Published).ToString(), Caption = "Visible on the platform" },
                new AdminSummaryCardViewModel { Label = "Draft Review", Value = allItems.Count(x => x.WorkflowStatus == PublicationWorkflowStatuses.Draft && x.RequiresReview).ToString(), Caption = "Need editorial review" },
                new AdminSummaryCardViewModel { Label = "Updated This Month", Value = allItems.Count(x => x.UpdatedAtUtc >= monthStart).ToString(), Caption = "Recently touched items" }
            ],
            AvailableTypes = allItems.Select(x => x.Type).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Order().ToList(),
            AvailableCountries = allItems
                .SelectMany(x => x.CountryAssignments.Count == 0 ? [x.CountryLabel] : x.CountryAssignments.Select(item => item.CountryLabel))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Order()
                .ToList(),
            AvailableTopics = allItems
                .SelectMany(x => x.CategoryAssignments.Count == 0 ? [x.Topic] : x.CategoryAssignments.Select(item => item.CategoryName))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Order()
                .ToList(),
            AvailableSources = allItems
                .Select(x => string.IsNullOrWhiteSpace(x.SourceName) ? x.SourceDomain : x.SourceName)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .Order()
                .ToList()!,
            Search = search,
            Type = type,
            Country = country,
            Topic = topic,
            Source = source,
            Status = status,
            DateFrom = dateFrom,
            DateTo = dateTo,
            ImportedDraftCount = importedDraftCount,
            ReviewRequiredCount = reviewRequiredCount,
            ImportedTodayCount = importedTodayCount
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
        var providedGalleryFiles = ResolveFiles(model.GalleryFiles, Request.Form.Files.GetFiles(nameof(model.GalleryFiles)));
        var savedGalleryImages = await _mediaStorage.SaveImagesAsync(providedGalleryFiles, "publications", HttpContext.RequestAborted);
        var galleryItems = ContentImageTextSerializer.Parse(model.GalleryText).ToList();
        galleryItems.AddRange(savedGalleryImages);
        var uploadedDocumentFiles = ResolveFiles(model.DocumentFiles, Request.Form.Files.GetFiles(nameof(model.DocumentFiles)));

        var entity = new PublicationContentItem
        {
            Id = Guid.NewGuid(),
            Slug = model.Slug.Trim(),
            WorkflowStatus = PublicationWorkflowStatuses.Normalize(model.WorkflowStatus),
            RequiresReview = model.RequiresReview,
            ExternalId = string.IsNullOrWhiteSpace(model.ExternalId) ? null : model.ExternalId.Trim(),
            SourceName = string.IsNullOrWhiteSpace(model.SourceName) ? null : model.SourceName.Trim(),
            SourceUrl = string.IsNullOrWhiteSpace(model.SourceUrl) ? null : PublicationImportHelpers.NormalizeSourceUrl(model.SourceUrl),
            SourceDomain = string.IsNullOrWhiteSpace(model.SourceUrl) ? null : PublicationImportHelpers.ExtractDomain(model.SourceUrl),
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
            DetailsJson = null,
            DisplayOrder = model.DisplayOrder,
            IsPublished = PublicationWorkflowStatuses.IsPublishedStatus(model.WorkflowStatus),
            SourcePublishedAtUtc = string.IsNullOrWhiteSpace(model.SourcePublishedAtUtc) ? null : DateTimeOffset.Parse(model.SourcePublishedAtUtc),
            ImportedAtUtc = DateTimeOffset.UtcNow,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        entity.DetailsJson = BuildDetailsJson(entity, model);

        _dbContext.PublicationContentItems.Add(entity);
        await ReplaceAssignmentsAsync(entity.Id, model.AssignedCountriesText, model.AssignedCategoriesText);
        var uploadedDocuments = await BuildUploadedPublicationDocumentsAsync(entity.Id, uploadedDocumentFiles);
        await ReplaceStructuredDetailsAsync(entity.Id, model, uploadedDocuments);
        await _dbContext.SaveChangesAsync();

        var uploadWarning = BuildFileUploadWarning(uploadedHeroFile, heroImageUrl, providedGalleryFiles.Count, savedGalleryImages.Count, uploadedDocumentFiles.Count, uploadedDocuments.Count);
        if (uploadWarning is not null)
        {
            TempData["AdminError"] = uploadWarning;
        }

        TempData["AdminSuccess"] = $"Publication saved. Hero: {(string.IsNullOrWhiteSpace(entity.HeroImageUrl) ? "not saved" : entity.HeroImageUrl)}. Gallery items: {galleryItems.Count}.";
        return RedirectToAction(nameof(Edit), new { id = entity.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.PublicationContentItems
            .Include(x => x.CountryAssignments)
            .Include(x => x.CategoryAssignments)
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
        var providedHeroFile = ResolveSingleFile(model.HeroImageFile, Request.Form.Files.GetFile(nameof(model.HeroImageFile)));
        var uploadedHeroImageUrl = await _mediaStorage.SaveImageAsync(providedHeroFile, "publications", HttpContext.RequestAborted);
        var providedGalleryFiles = ResolveFiles(model.GalleryFiles, Request.Form.Files.GetFiles(nameof(model.GalleryFiles)));
        var savedGalleryImages = await _mediaStorage.SaveImagesAsync(providedGalleryFiles, "publications", HttpContext.RequestAborted);
        var galleryItems = ContentImageTextSerializer.Parse(model.GalleryText).ToList();
        galleryItems.AddRange(savedGalleryImages);
        var uploadedDocumentFiles = ResolveFiles(model.DocumentFiles, Request.Form.Files.GetFiles(nameof(model.DocumentFiles)));

        entity.Slug = model.Slug.Trim();
        entity.WorkflowStatus = PublicationWorkflowStatuses.Normalize(model.WorkflowStatus);
        entity.RequiresReview = model.RequiresReview;
        entity.ExternalId = string.IsNullOrWhiteSpace(model.ExternalId) ? entity.ExternalId : model.ExternalId.Trim();
        entity.SourceName = string.IsNullOrWhiteSpace(model.SourceName) ? entity.SourceName : model.SourceName.Trim();
        entity.SourceUrl = string.IsNullOrWhiteSpace(model.SourceUrl) ? entity.SourceUrl : PublicationImportHelpers.NormalizeSourceUrl(model.SourceUrl);
        entity.SourceDomain = string.IsNullOrWhiteSpace(model.SourceUrl) ? entity.SourceDomain : PublicationImportHelpers.ExtractDomain(model.SourceUrl);
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
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsPublished = PublicationWorkflowStatuses.IsPublishedStatus(model.WorkflowStatus);
        entity.SourcePublishedAtUtc = string.IsNullOrWhiteSpace(model.SourcePublishedAtUtc)
            ? entity.SourcePublishedAtUtc
            : DateTimeOffset.Parse(model.SourcePublishedAtUtc);
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        entity.DetailsJson = BuildDetailsJson(entity, model);

        await ReplaceAssignmentsAsync(entity.Id, model.AssignedCountriesText, model.AssignedCategoriesText);
        var uploadedDocuments = await BuildUploadedPublicationDocumentsAsync(entity.Id, uploadedDocumentFiles);
        await ReplaceStructuredDetailsAsync(entity.Id, model, uploadedDocuments);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(previousUrls.Except(GetPublicationMediaUrls(entity), StringComparer.OrdinalIgnoreCase));

        var uploadWarning = BuildFileUploadWarning(providedHeroFile, uploadedHeroImageUrl, providedGalleryFiles.Count, savedGalleryImages.Count, uploadedDocumentFiles.Count, uploadedDocuments.Count);
        if (uploadWarning is not null)
        {
            TempData["AdminError"] = uploadWarning;
        }

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, string status)
    {
        var entity = await _dbContext.PublicationContentItems.FindAsync(id);
        if (entity is null)
        {
            return NotFound();
        }

        entity.WorkflowStatus = PublicationWorkflowStatuses.Normalize(status);
        entity.IsPublished = PublicationWorkflowStatuses.IsPublishedStatus(entity.WorkflowStatus);
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync();

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
                await _mediaStorage.DeleteImageAsync(url);
            }
        }
    }

    private PublicationContentEditViewModel BuildEditViewModel(PublicationContentItem entity)
    {
        var jsonDetails = DeserializeDetails(entity.DetailsJson);
        var fallbackDetails = jsonDetails is null
            ? BuildDefaultDetails(entity)
            : MergeWithFallback(jsonDetails, BuildDefaultDetails(entity), entity.DetailsJson);
        var details = HasStructuredDetails(entity)
            ? BuildStructuredDetails(entity, fallbackDetails)
            : fallbackDetails;

        return new PublicationContentEditViewModel
        {
            Id = entity.Id,
            Slug = entity.Slug,
            WorkflowStatus = PublicationWorkflowStatuses.Normalize(entity.WorkflowStatus),
            AvailableStatuses = PublicationWorkflowStatuses.All,
            RequiresReview = entity.RequiresReview,
            ExternalId = entity.ExternalId,
            SourceName = entity.SourceName,
            SourceUrl = entity.SourceUrl,
            SourceDomain = entity.SourceDomain,
            SourcePublishedAtUtc = entity.SourcePublishedAtUtc?.ToString("yyyy-MM-ddTHH:mm:sszzz"),
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
            ActionsText = PublicationStructuredDetailParsers.JoinActions(DefaultPublicationActions.Select(label => new PublicationActionItemViewModel { Label = label })),
            ExecutiveSummaryText = !string.IsNullOrWhiteSpace(details.ExecutiveSummaryHtml)
                ? details.ExecutiveSummaryHtml
                : PublicationStructuredDetailParsers.JoinParagraphs(details.ExecutiveSummaryParagraphs),
            ExecutiveHighlightsText = PublicationStructuredDetailParsers.JoinParagraphs(details.ExecutiveHighlights),
            KeyFindingsText = PublicationStructuredDetailParsers.JoinKeyFindings(details.KeyFindings),
            SectionsText = PublicationStructuredDetailParsers.JoinSections(details.Sections),
            ChartsText = PublicationStructuredDetailParsers.JoinCharts(details.Charts),
            TableRowsText = PublicationStructuredDetailParsers.JoinTableRows(details.TableRows),
            // Sourced directly from entity.GalleryJson (the actual saved gallery), not from
            // details.GalleryItems - that traces through BuildDefaultDetails/BuildStructuredDetails,
            // which never populate GalleryItems from the entity, so it was always empty here and
            // editing a publication silently wiped its gallery on save.
            GalleryText = entity.GalleryJson ?? string.Empty,
            AssignedCountriesText = entity.CountryAssignments.Count == 0
                ? entity.CountryLabel
                : string.Join(Environment.NewLine, entity.CountryAssignments.OrderBy(x => x.DisplayOrder).Select(x => x.CountryLabel)),
            AssignedCategoriesText = entity.CategoryAssignments.Count == 0
                ? entity.Topic
                : string.Join(Environment.NewLine, entity.CategoryAssignments.OrderBy(x => x.DisplayOrder).Select(x => x.CategoryName)),
            DocumentsText = PublicationStructuredDetailParsers.JoinDocuments(details.Documents),
            DocumentUploadMapText = string.Empty,
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
            WorkflowStatus = string.IsNullOrWhiteSpace(model.WorkflowStatus) ? PublicationWorkflowStatuses.Draft : model.WorkflowStatus,
            AvailableStatuses = PublicationWorkflowStatuses.All,
            RequiresReview = model.RequiresReview,
            ExternalId = model.ExternalId,
            SourceName = model.SourceName,
            SourceUrl = model.SourceUrl,
            SourceDomain = model.SourceDomain,
            SourcePublishedAtUtc = model.SourcePublishedAtUtc,
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
            ChartsText = model.ChartsText,
            TableRowsText = model.TableRowsText,
            GalleryText = model.GalleryText,
            AssignedCountriesText = model.AssignedCountriesText,
            AssignedCategoriesText = model.AssignedCategoriesText,
            DocumentsText = model.DocumentsText,
            DocumentUploadMapText = model.DocumentUploadMapText,
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
            ExecutiveSummaryHtml = summarySection is not null && LooksLikeHtml(summarySection.ParagraphsText)
                ? summarySection.ParagraphsText
                : string.Empty,
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
            PdfDownloadUrl = string.IsNullOrWhiteSpace(details.PdfDownloadUrl) ? fallback.PdfDownloadUrl : details.PdfDownloadUrl,
            ExecutiveSummaryHtml = string.IsNullOrWhiteSpace(details.ExecutiveSummaryHtml) ? fallback.ExecutiveSummaryHtml : details.ExecutiveSummaryHtml,
            IsPremium = details.IsPremium || fallback.IsPremium,
            CanAccessFullContent = details.CanAccessFullContent || fallback.CanAccessFullContent,
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

    private static string? BuildDetailsJson(
        PublicationContentItem entity,
        PublicationContentEditViewModel model)
    {
        var existingDetails = DeserializeDetails(entity.DetailsJson);
        var baseDetails = existingDetails is null
            ? BuildDefaultDetails(entity)
            : MergeWithFallback(existingDetails, BuildDefaultDetails(entity), entity.DetailsJson);

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
            HeroImageUrl = entity.HeroImageUrl ?? string.Empty,
            PdfDownloadUrl = entity.PdfDownloadUrl ?? "#",
            ExecutiveSummaryHtml = baseDetails.ExecutiveSummaryHtml,
            IsPremium = baseDetails.IsPremium,
            CanAccessFullContent = baseDetails.CanAccessFullContent,
            Actions = baseDetails.Actions,
            Tabs = baseDetails.Tabs,
            Kpis = baseDetails.Kpis,
            ExecutiveSummaryParagraphs = baseDetails.ExecutiveSummaryParagraphs,
            ExecutiveHighlights = baseDetails.ExecutiveHighlights,
            KeyFindings = baseDetails.KeyFindings,
            Sections = baseDetails.Sections,
            Charts = PublicationStructuredDetailParsers.ParseCharts(model.ChartsText),
            TableRows = PublicationStructuredDetailParsers.ParseTableRows(model.TableRowsText),
            MapRoutes = baseDetails.MapRoutes,
            GalleryItems = baseDetails.GalleryItems,
            Documents = baseDetails.Documents,
            TableOfContents = baseDetails.TableOfContents,
            KeyTopics = baseDetails.KeyTopics,
            SidebarDocuments = baseDetails.SidebarDocuments,
            RelatedIntelligence = baseDetails.RelatedIntelligence,
            PublicationInfo = baseDetails.PublicationInfo,
            RelatedPublications = baseDetails.RelatedPublications
        };

        return JsonSerializer.Serialize(details, JsonOptions);
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
            ExecutiveSummaryHtml = string.Empty,
            Actions = DefaultPublicationActions.Select(label => new PublicationActionItemViewModel { Label = label }).ToList(),
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

    private static IReadOnlyList<int> ParseDocumentUploadMap(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return raw
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(value => int.TryParse(value, out var index) ? index : (int?)null)
            .Where(value => value.HasValue)
            .Select(value => value!.Value)
            .ToList();
    }

    private async Task ReplaceStructuredDetailsAsync(
        Guid publicationContentItemId,
        PublicationContentEditViewModel model,
        IReadOnlyList<UploadedPublicationDocumentFile>? uploadedDocuments = null)
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
            DefaultPublicationActions.Select((label, index) => new PublicationActionEntry
            {
                Id = Guid.NewGuid(),
                PublicationContentItemId = publicationContentItemId,
                Label = label,
                DisplayOrder = index
            }));

        if (!string.IsNullOrWhiteSpace(model.ExecutiveSummaryText))
        {
            _dbContext.PublicationArticleSections.Add(new PublicationArticleSection
            {
                Id = Guid.NewGuid(),
                PublicationContentItemId = publicationContentItemId,
                SectionKey = "executive-summary",
                Heading = "Executive Summary",
                ParagraphsText = SanitizeRichTextHtml(model.ExecutiveSummaryText),
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
        var uploadMap = ParseDocumentUploadMap(model.DocumentUploadMapText);
        var uploadsByRowIndex = new Dictionary<int, UploadedPublicationDocumentFile>();

        if (uploadedDocuments is not null && uploadedDocuments.Count != 0)
        {
            for (var index = 0; index < uploadedDocuments.Count && index < uploadMap.Count; index++)
            {
                uploadsByRowIndex[uploadMap[index]] = uploadedDocuments[index];
            }
        }

        var savedDocuments = new List<PublicationDocumentEntry>();
        for (var index = 0; index < documentRows.Count; index++)
        {
            var parts = documentRows[index];
            uploadsByRowIndex.TryGetValue(index, out var uploadedDocument);

            savedDocuments.Add(new PublicationDocumentEntry
            {
                Id = Guid.NewGuid(),
                PublicationContentItemId = publicationContentItemId,
                Title = string.IsNullOrWhiteSpace(parts[0]) ? uploadedDocument?.Title ?? "Publication document" : parts[0],
                Type = string.IsNullOrWhiteSpace(parts[1]) ? uploadedDocument?.Type ?? string.Empty : parts[1],
                Meta = string.IsNullOrWhiteSpace(parts[2]) ? uploadedDocument?.Meta ?? string.Empty : parts[2],
                DownloadUrl = uploadedDocument?.DownloadUrl ?? (parts.Length > 3 ? string.Join(" | ", parts.Skip(3)) : "#"),
                DisplayOrder = index
            });
        }

        if (uploadedDocuments is not null && uploadedDocuments.Count != 0)
        {
            foreach (var item in uploadedDocuments.Select((document, index) => (document, index)))
            {
                if (item.index < uploadMap.Count)
                {
                    continue;
                }

                savedDocuments.Add(new PublicationDocumentEntry
                {
                    Id = Guid.NewGuid(),
                    PublicationContentItemId = publicationContentItemId,
                    Title = item.document.Title,
                    Type = item.document.Type,
                    Meta = item.document.Meta,
                    DownloadUrl = item.document.DownloadUrl,
                    DisplayOrder = savedDocuments.Count
                });
            }
        }

        _dbContext.PublicationDocumentEntries.AddRange(savedDocuments);

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
