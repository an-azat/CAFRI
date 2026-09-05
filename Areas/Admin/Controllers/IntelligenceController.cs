using System.Text.Json;
using CAFRI.Areas.Admin.ViewModels.Content;
using CAFRI.Domain.Content;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Intelligence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class IntelligenceController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly ContentMediaStorageService _mediaStorage;

    public IntelligenceController(AppDbContext dbContext, ContentMediaStorageService mediaStorage)
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

    private async Task<IReadOnlyList<IntelligenceOfficialDocumentEntry>> BuildUploadedIntelligenceDocumentsAsync(
        Guid intelligenceId,
        IReadOnlyList<IFormFile> files)
    {
        if (files.Count == 0)
        {
            return [];
        }

        var items = new List<IntelligenceOfficialDocumentEntry>();
        foreach (var file in files)
        {
            var url = await _mediaStorage.SaveDocumentAsync(file, "intelligence", HttpContext.RequestAborted);
            if (string.IsNullOrWhiteSpace(url))
            {
                continue;
            }

            var title = Path.GetFileNameWithoutExtension(file.FileName)
                .Replace('_', ' ')
                .Replace('-', ' ')
                .Trim();
            var type = BuildDocumentTypeFromExtension(file.FileName);

            items.Add(new IntelligenceOfficialDocumentEntry
            {
                Id = Guid.NewGuid(),
                IntelligenceContentItemId = intelligenceId,
                Title = string.IsNullOrWhiteSpace(title) ? "Official document" : title,
                Subtitle = "Uploaded file",
                Meta = $"{type} | {FormatFileSize(file.Length)}",
                DownloadUrl = url,
                DisplayOrder = items.Count
            });
        }

        return items;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category, string? country, string? status)
    {
        ViewData["Title"] = "Admin Intelligence";
        ViewData["AdminNav"] = "intelligence";

        var query = _dbContext.IntelligenceContentItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Title, $"%{term}%") ||
                EF.Functions.ILike(x.Description, $"%{term}%") ||
                EF.Functions.ILike(x.Category, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(x => x.CountryName == country);
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

        var allItems = await _dbContext.IntelligenceContentItems.AsNoTracking().ToListAsync();
        var monthStart = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);

        return View(new IntelligenceIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Materials", Value = allItems.Count.ToString(), Caption = "Database records" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.IsPublished).ToString(), Caption = "Visible on the platform" },
                new AdminSummaryCardViewModel { Label = "Drafts", Value = allItems.Count(x => !x.IsPublished).ToString(), Caption = "Work in progress" },
                new AdminSummaryCardViewModel { Label = "Updated This Month", Value = allItems.Count(x => x.UpdatedAtUtc >= monthStart).ToString(), Caption = "Recently touched items" }
            ],
            AvailableCategories = allItems.Select(x => x.Category).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Order().ToList(),
            AvailableCountries = allItems.Select(x => x.CountryName).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().Order().ToList(),
            Search = search,
            Category = category,
            Country = country,
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Intelligence Item";
        ViewData["AdminNav"] = "intelligence";
        return View("Edit", BuildEditViewModel(new IntelligenceContentEditViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IntelligenceContentEditViewModel model)
    {
        var uploadedHeroFile = ResolveSingleFile(model.HeroImageFile, Request.Form.Files.GetFile(nameof(model.HeroImageFile)));
        if (uploadedHeroFile is not null && !_mediaStorage.IsSupportedImage(uploadedHeroFile))
        {
            ModelState.AddModelError(nameof(model.HeroImageFile), $"Unsupported hero image format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }

        var uploadedDocumentFiles = ResolveFiles(model.DocumentFiles, Request.Form.Files.GetFiles(nameof(model.DocumentFiles)));
        if (uploadedDocumentFiles.Any(file => !_mediaStorage.IsSupportedDocument(file)))
        {
            ModelState.AddModelError(nameof(model.DocumentFiles), $"One or more document files use an unsupported format. Allowed: {_mediaStorage.GetAllowedDocumentExtensionsLabel()}");
        }

        if (!ModelState.IsValid) return View("Edit", BuildEditViewModel(model));
        var heroImageUrl = string.IsNullOrWhiteSpace(model.HeroImageUrl)
            ? await _mediaStorage.SaveImageAsync(uploadedHeroFile, "intelligence", HttpContext.RequestAborted)
            : model.HeroImageUrl.Trim();
        var entity = new IntelligenceContentItem
        {
            Id = Guid.NewGuid(),
            Slug = model.Slug.Trim(),
            Category = model.Category.Trim(),
            CountryCode = model.CountryCode.Trim(),
            CountryName = model.CountryName.Trim(),
            PublishedLabel = model.PublishedLabel.Trim(),
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Source = model.Source.Trim(),
            HeroImageUrl = heroImageUrl,
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        _dbContext.IntelligenceContentItems.Add(entity);
        await _dbContext.SaveChangesAsync();
        var uploadedDocuments = await BuildUploadedIntelligenceDocumentsAsync(entity.Id, uploadedDocumentFiles);
        await ReplaceStructuredDetailsAsync(entity.Id, model, uploadedDocuments);
        await _dbContext.SaveChangesAsync();
        TempData["AdminSuccess"] = $"Intelligence item saved. Hero: {(string.IsNullOrWhiteSpace(entity.HeroImageUrl) ? "not saved" : entity.HeroImageUrl)}.";
        return RedirectToAction(nameof(Edit), new { id = entity.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.IntelligenceContentItems
            .Include(x => x.Sections)
            .Include(x => x.KeyChangeEntries)
            .Include(x => x.ImpactEntries)
            .Include(x => x.TimelineEntries)
            .Include(x => x.DocumentInfoEntries)
            .Include(x => x.OfficialDocuments)
            .Include(x => x.RelatedLinks)
            .Include(x => x.StatusEntries)
            .Include(x => x.HighlightEntries)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        ViewData["Title"] = "Edit Intelligence Item";
        ViewData["AdminNav"] = "intelligence";
        return View(BuildEditViewModel(entity));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, IntelligenceContentEditViewModel model)
    {
        var uploadedHeroFile = ResolveSingleFile(model.HeroImageFile, Request.Form.Files.GetFile(nameof(model.HeroImageFile)));
        if (uploadedHeroFile is not null && !_mediaStorage.IsSupportedImage(uploadedHeroFile))
        {
            ModelState.AddModelError(nameof(model.HeroImageFile), $"Unsupported hero image format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }

        var uploadedDocumentFiles = ResolveFiles(model.DocumentFiles, Request.Form.Files.GetFiles(nameof(model.DocumentFiles)));
        if (uploadedDocumentFiles.Any(file => !_mediaStorage.IsSupportedDocument(file)))
        {
            ModelState.AddModelError(nameof(model.DocumentFiles), $"One or more document files use an unsupported format. Allowed: {_mediaStorage.GetAllowedDocumentExtensionsLabel()}");
        }

        if (!ModelState.IsValid) return View(BuildEditViewModel(model));
        var entity = await _dbContext.IntelligenceContentItems.FindAsync(id);
        if (entity is null) return NotFound();
        var previousHeroImageUrl = entity.HeroImageUrl;
        var uploadedHeroImageUrl = await _mediaStorage.SaveImageAsync(uploadedHeroFile, "intelligence", HttpContext.RequestAborted);

        entity.Slug = model.Slug.Trim();
        entity.Category = model.Category.Trim();
        entity.CountryCode = model.CountryCode.Trim();
        entity.CountryName = model.CountryName.Trim();
        entity.PublishedLabel = model.PublishedLabel.Trim();
        entity.Title = model.Title.Trim();
        entity.Description = model.Description.Trim();
        entity.Source = model.Source.Trim();
        entity.HeroImageUrl = !string.IsNullOrWhiteSpace(uploadedHeroImageUrl)
            ? uploadedHeroImageUrl
            : string.IsNullOrWhiteSpace(model.HeroImageUrl) ? entity.HeroImageUrl : model.HeroImageUrl.Trim();
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsPublished = model.IsPublished;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        var uploadedDocuments = await BuildUploadedIntelligenceDocumentsAsync(entity.Id, uploadedDocumentFiles);
        await ReplaceStructuredDetailsAsync(entity.Id, model, uploadedDocuments);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(previousHeroImageUrl, entity.HeroImageUrl);
        TempData["AdminSuccess"] = $"Intelligence item updated. Hero: {(string.IsNullOrWhiteSpace(entity.HeroImageUrl) ? "not saved" : entity.HeroImageUrl)}.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _dbContext.IntelligenceContentItems.FindAsync(id);
        if (entity is null) return NotFound();
        var deletedHeroImageUrl = entity.HeroImageUrl;
        _dbContext.IntelligenceContentItems.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(deletedHeroImageUrl, null);
        return RedirectToAction(nameof(Index));
    }

    private async Task DeleteUnusedMediaAsync(string? previousUrl, string? currentUrl)
    {
        if (string.IsNullOrWhiteSpace(previousUrl) ||
            string.Equals(previousUrl, currentUrl, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var stillUsed = await _dbContext.CountryContents.AsNoTracking().AnyAsync(x => x.HeroGalleryJson != null && x.HeroGalleryJson.Contains(previousUrl)) ||
                        await _dbContext.PublicationContentItems.AsNoTracking().AnyAsync(x =>
                            x.HeroImageUrl == previousUrl ||
                            (x.GalleryJson != null && x.GalleryJson.Contains(previousUrl))) ||
                        await _dbContext.IntelligenceContentItems.AsNoTracking().AnyAsync(x => x.HeroImageUrl == previousUrl);

        if (!stillUsed)
        {
            await _mediaStorage.DeleteImageAsync(previousUrl);
        }
    }

    private IntelligenceContentEditViewModel BuildEditViewModel(IntelligenceContentItem entity)
    {
        var details = DeserializeDetails(entity.DetailsJson);
        var structuredRelatedOfficialSources = entity.RelatedLinks
            .Where(x => x.LinkType == "OfficialSource")
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new IntelligenceSidebarLinkViewModel { Title = x.Title, Subtitle = x.Subtitle, Url = x.Url })
            .ToList();
        var structuredRelatedIntelligence = entity.RelatedLinks
            .Where(x => x.LinkType == "RelatedIntelligence")
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new IntelligenceSidebarLinkViewModel { Title = x.Title, Subtitle = x.Subtitle, Url = x.Url })
            .ToList();

        return new IntelligenceContentEditViewModel
        {
            Id = entity.Id,
            Slug = entity.Slug,
            Category = entity.Category,
            CountryCode = entity.CountryCode,
            CountryName = entity.CountryName,
            PublishedLabel = entity.PublishedLabel,
            Title = entity.Title,
            Description = entity.Description,
            Source = entity.Source,
            HeroImageUrl = entity.HeroImageUrl ?? string.Empty,
            DetailsJson = entity.DetailsJson,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished,
            OverviewLead = entity.Sections.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.SectionKey == "overview")?.Body ?? details?.OverviewLead ?? string.Empty,
            OverviewBody = entity.Sections.Where(x => x.SectionKey == "overview").OrderBy(x => x.DisplayOrder).Skip(1).FirstOrDefault()?.Body ?? details?.OverviewBody ?? string.Empty,
            EffectiveDate = entity.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.Label == "Effective Date")?.Value ?? details?.EffectiveDate ?? string.Empty,
            AppliesTo = entity.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).FirstOrDefault(x => x.Label == "Applies To")?.Value ?? details?.AppliesTo ?? string.Empty,
            DocumentTitle = entity.OfficialDocuments.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.Title ?? details?.OfficialDocument.Title ?? string.Empty,
            DocumentSubtitle = entity.OfficialDocuments.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.Subtitle ?? details?.OfficialDocument.Subtitle ?? string.Empty,
            DocumentMeta = entity.OfficialDocuments.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.Meta ?? details?.OfficialDocument.Meta ?? string.Empty,
            DocumentDownloadUrl = entity.OfficialDocuments.OrderBy(x => x.DisplayOrder).FirstOrDefault()?.DownloadUrl ?? details?.OfficialDocument.DownloadUrl ?? string.Empty,
            DocumentsText = entity.OfficialDocuments.Count != 0
                ? string.Join(Environment.NewLine, entity.OfficialDocuments.OrderBy(x => x.DisplayOrder).Select(x => $"{x.Title} | {x.Subtitle} | {x.Meta} | {x.DownloadUrl}"))
                : string.Empty,
            KeyChangesText = entity.KeyChangeEntries.Count != 0
                ? string.Join(Environment.NewLine, entity.KeyChangeEntries.OrderBy(x => x.DisplayOrder).Select(x => $"{x.Number} | {x.Title} | {x.Description}"))
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinKeyChanges(details.KeyChanges),
            ImpactAnalysisText = entity.ImpactEntries.Count != 0
                ? string.Join(Environment.NewLine, entity.ImpactEntries.OrderBy(x => x.DisplayOrder).Select(x => $"{x.Title} | {x.Description} | {x.ImpactLabel} | {x.ImpactTone}"))
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinImpactAnalysis(details.ImpactAnalysis),
            TimelineText = entity.TimelineEntries.Count != 0
                ? string.Join(Environment.NewLine, entity.TimelineEntries.OrderBy(x => x.DisplayOrder).Select(x => $"{x.DateLabel} | {x.Description} | {x.Stage}"))
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinTimeline(details.TimelineItems),
            DocumentInformationText = entity.DocumentInfoEntries.Count != 0
                ? string.Join(Environment.NewLine, entity.DocumentInfoEntries.OrderBy(x => x.DisplayOrder).Select(x => $"{x.Label} | {x.Value}"))
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinDocumentInfo(details.DocumentInformation),
            KeyHighlightsText = entity.HighlightEntries.Count != 0
                ? string.Join(Environment.NewLine, entity.HighlightEntries.OrderBy(x => x.DisplayOrder).Select(x => x.Text))
                : details is null ? string.Empty : string.Join(Environment.NewLine, details.KeyHighlights),
            RelatedOfficialSourcesText = structuredRelatedOfficialSources.Count != 0
                ? IntelligenceStructuredDetailParsers.JoinLinks(structuredRelatedOfficialSources)
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinLinks(details.RelatedOfficialSources),
            RelatedIntelligenceText = structuredRelatedIntelligence.Count != 0
                ? IntelligenceStructuredDetailParsers.JoinLinks(structuredRelatedIntelligence)
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinLinks(details.RelatedIntelligence),
            MonitoringStatusText = entity.StatusEntries.Count != 0
                ? string.Join(Environment.NewLine, entity.StatusEntries.OrderBy(x => x.DisplayOrder).Select(x => $"{x.Label} | {x.Value} | {x.IsStatus}"))
                : details is null ? string.Empty : IntelligenceStructuredDetailParsers.JoinStatus(details.MonitoringStatus)
        };
    }

    private static IntelligenceContentEditViewModel BuildEditViewModel(IntelligenceContentEditViewModel model) =>
        new()
        {
            Id = model.Id,
            Slug = model.Slug,
            Category = model.Category,
            CountryCode = model.CountryCode,
            CountryName = model.CountryName,
            PublishedLabel = model.PublishedLabel,
            Title = model.Title,
            Description = model.Description,
            Source = model.Source,
            HeroImageUrl = model.HeroImageUrl,
            DetailsJson = model.DetailsJson,
            OverviewLead = model.OverviewLead,
            OverviewBody = model.OverviewBody,
            EffectiveDate = model.EffectiveDate,
            AppliesTo = model.AppliesTo,
            DocumentTitle = model.DocumentTitle,
            DocumentSubtitle = model.DocumentSubtitle,
            DocumentMeta = model.DocumentMeta,
            DocumentDownloadUrl = model.DocumentDownloadUrl,
            DocumentsText = model.DocumentsText,
            KeyChangesText = model.KeyChangesText,
            ImpactAnalysisText = model.ImpactAnalysisText,
            TimelineText = model.TimelineText,
            DocumentInformationText = model.DocumentInformationText,
            KeyHighlightsText = model.KeyHighlightsText,
            RelatedOfficialSourcesText = model.RelatedOfficialSourcesText,
            RelatedIntelligenceText = model.RelatedIntelligenceText,
            MonitoringStatusText = model.MonitoringStatusText,
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished
        };

    private IntelligenceDetailsViewModel? DeserializeDetails(string? detailsJson)
    {
        if (string.IsNullOrWhiteSpace(detailsJson))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<IntelligenceDetailsViewModel>(detailsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return null;
        }
    }

    private async Task ReplaceStructuredDetailsAsync(
        Guid intelligenceContentItemId,
        IntelligenceContentEditViewModel model,
        IReadOnlyList<IntelligenceOfficialDocumentEntry>? uploadedDocuments = null)
    {
        var existingSections = await _dbContext.IntelligenceArticleSections.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingKeyChanges = await _dbContext.IntelligenceKeyChangeEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingImpacts = await _dbContext.IntelligenceImpactEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingTimeline = await _dbContext.IntelligenceTimelineEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingDocumentInfo = await _dbContext.IntelligenceDocumentInfoEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingDocuments = await _dbContext.IntelligenceOfficialDocumentEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingLinks = await _dbContext.IntelligenceRelatedLinks.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingStatuses = await _dbContext.IntelligenceStatusEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();
        var existingHighlights = await _dbContext.IntelligenceHighlightEntries.Where(x => x.IntelligenceContentItemId == intelligenceContentItemId).ToListAsync();

        _dbContext.IntelligenceArticleSections.RemoveRange(existingSections);
        _dbContext.IntelligenceKeyChangeEntries.RemoveRange(existingKeyChanges);
        _dbContext.IntelligenceImpactEntries.RemoveRange(existingImpacts);
        _dbContext.IntelligenceTimelineEntries.RemoveRange(existingTimeline);
        _dbContext.IntelligenceDocumentInfoEntries.RemoveRange(existingDocumentInfo);
        _dbContext.IntelligenceOfficialDocumentEntries.RemoveRange(existingDocuments);
        _dbContext.IntelligenceRelatedLinks.RemoveRange(existingLinks);
        _dbContext.IntelligenceStatusEntries.RemoveRange(existingStatuses);
        _dbContext.IntelligenceHighlightEntries.RemoveRange(existingHighlights);

        var sections = new List<IntelligenceArticleSection>();
        if (!string.IsNullOrWhiteSpace(model.OverviewLead))
        {
            sections.Add(new IntelligenceArticleSection
            {
                Id = Guid.NewGuid(),
                IntelligenceContentItemId = intelligenceContentItemId,
                SectionKey = "overview",
                Heading = "Overview",
                Body = model.OverviewLead.Trim(),
                DisplayOrder = 0
            });
        }

        if (!string.IsNullOrWhiteSpace(model.OverviewBody))
        {
            sections.Add(new IntelligenceArticleSection
            {
                Id = Guid.NewGuid(),
                IntelligenceContentItemId = intelligenceContentItemId,
                SectionKey = "overview",
                Heading = "Overview",
                Body = model.OverviewBody.Trim(),
                DisplayOrder = 1
            });
        }

        _dbContext.IntelligenceArticleSections.AddRange(sections);

        var keyChangeRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.KeyChangesText, 3);
        _dbContext.IntelligenceKeyChangeEntries.AddRange(keyChangeRows.Select((parts, index) => new IntelligenceKeyChangeEntry
        {
            Id = Guid.NewGuid(),
            IntelligenceContentItemId = intelligenceContentItemId,
            Number = int.TryParse(parts[0], out var number) ? number : index + 1,
            Title = parts[1],
            Description = string.Join(" | ", parts.Skip(2)),
            DisplayOrder = index
        }));

        // Title(0) | Description(1..) | ImpactLabel | ImpactTone
        // Description is the free-text field: a "|" typed inside it folds back into
        // Description instead of shifting ImpactLabel/ImpactTone out of position.
        var impactRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.ImpactAnalysisText, 4);
        _dbContext.IntelligenceImpactEntries.AddRange(impactRows.Select((parts, index) =>
        {
            var descriptionEndIndex = parts.Length - 2;
            return new IntelligenceImpactEntry
            {
                Id = Guid.NewGuid(),
                IntelligenceContentItemId = intelligenceContentItemId,
                Title = parts[0],
                Description = string.Join(" | ", parts.Skip(1).Take(descriptionEndIndex - 1)),
                ImpactLabel = parts[descriptionEndIndex],
                ImpactTone = parts[descriptionEndIndex + 1],
                DisplayOrder = index
            };
        }));

        // DateLabel(0) | Description(1..) | Stage
        // Same fold-back rule as above, applied to Description.
        var timelineRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.TimelineText, 3);
        _dbContext.IntelligenceTimelineEntries.AddRange(timelineRows.Select((parts, index) =>
        {
            var descriptionEndIndex = parts.Length - 1;
            return new IntelligenceTimelineEntry
            {
                Id = Guid.NewGuid(),
                IntelligenceContentItemId = intelligenceContentItemId,
                DateLabel = parts[0],
                Description = string.Join(" | ", parts.Skip(1).Take(descriptionEndIndex - 1)),
                Stage = parts[descriptionEndIndex],
                DisplayOrder = index
            };
        }));

        var documentInfoRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.DocumentInformationText, 2).ToList();
        if (!string.IsNullOrWhiteSpace(model.EffectiveDate))
        {
            documentInfoRows.Insert(0, ["Effective Date", model.EffectiveDate.Trim()]);
        }

        if (!string.IsNullOrWhiteSpace(model.AppliesTo))
        {
            documentInfoRows.Insert(documentInfoRows.Count, ["Applies To", model.AppliesTo.Trim()]);
        }

        _dbContext.IntelligenceDocumentInfoEntries.AddRange(documentInfoRows.Select((parts, index) => new IntelligenceDocumentInfoEntry
        {
            Id = Guid.NewGuid(),
            IntelligenceContentItemId = intelligenceContentItemId,
            Label = parts[0],
            Value = string.Join(" | ", parts.Skip(1)),
            DisplayOrder = index
        }));

        var documentRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.DocumentsText, 3).ToList();
        if (documentRows.Count == 0 && !string.IsNullOrWhiteSpace(model.DocumentTitle))
        {
            documentRows.Add(
            [
                model.DocumentTitle.Trim(),
                model.DocumentSubtitle.Trim(),
                model.DocumentMeta.Trim(),
                string.IsNullOrWhiteSpace(model.DocumentDownloadUrl) ? "#" : model.DocumentDownloadUrl.Trim()
            ]);
        }

        _dbContext.IntelligenceOfficialDocumentEntries.AddRange(documentRows.Select((parts, index) => new IntelligenceOfficialDocumentEntry
        {
            Id = Guid.NewGuid(),
            IntelligenceContentItemId = intelligenceContentItemId,
            Title = parts[0],
            Subtitle = parts[1],
            Meta = parts[2],
            DownloadUrl = parts.Length > 3 ? string.Join(" | ", parts.Skip(3)) : "#",
            DisplayOrder = index
        }));

        if (uploadedDocuments is not null && uploadedDocuments.Count != 0)
        {
            foreach (var item in uploadedDocuments.Select((document, index) => (document, index)))
            {
                item.document.DisplayOrder = documentRows.Count + item.index;
            }

            _dbContext.IntelligenceOfficialDocumentEntries.AddRange(uploadedDocuments);
        }

        var highlightRows = model.KeyHighlightsText
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        _dbContext.IntelligenceHighlightEntries.AddRange(highlightRows.Select((text, index) => new IntelligenceHighlightEntry
        {
            Id = Guid.NewGuid(),
            IntelligenceContentItemId = intelligenceContentItemId,
            Text = text,
            DisplayOrder = index
        }));

        var officialSourceRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.RelatedOfficialSourcesText, 2);
        _dbContext.IntelligenceRelatedLinks.AddRange(officialSourceRows.Select((parts, index) => new IntelligenceRelatedLink
        {
            Id = Guid.NewGuid(),
            IntelligenceContentItemId = intelligenceContentItemId,
            LinkType = "OfficialSource",
            Title = parts[0],
            Subtitle = parts[1],
            Url = parts.Length > 2 ? string.Join(" | ", parts.Skip(2)) : null,
            DisplayOrder = index
        }));

        var relatedIntelligenceRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.RelatedIntelligenceText, 2);
        _dbContext.IntelligenceRelatedLinks.AddRange(relatedIntelligenceRows.Select((parts, index) => new IntelligenceRelatedLink
        {
            Id = Guid.NewGuid(),
            IntelligenceContentItemId = intelligenceContentItemId,
            LinkType = "RelatedIntelligence",
            Title = parts[0],
            Subtitle = parts[1],
            Url = parts.Length > 2 ? string.Join(" | ", parts.Skip(2)) : null,
            DisplayOrder = index
        }));

        // Label(0) | Value(1..) | [IsStatus]
        // The trailing segment is only treated as the IsStatus flag when it actually
        // parses as a bool; otherwise it's folded back into Value like the other
        // free-text fields above, so a "|" inside Value can't corrupt IsStatus.
        var statusRows = IntelligenceStructuredDetailParsers.ParsePipeRows(model.MonitoringStatusText, 2);
        _dbContext.IntelligenceStatusEntries.AddRange(statusRows.Select((parts, index) =>
        {
            var hasIsStatus = parts.Length > 2 && bool.TryParse(parts[^1], out _);
            var valueParts = hasIsStatus ? parts.Skip(1).Take(parts.Length - 2) : parts.Skip(1);

            return new IntelligenceStatusEntry
            {
                Id = Guid.NewGuid(),
                IntelligenceContentItemId = intelligenceContentItemId,
                Label = parts[0],
                Value = string.Join(" | ", valueParts),
                IsStatus = hasIsStatus && bool.Parse(parts[^1]),
                DisplayOrder = index
            };
        }));
    }
}
