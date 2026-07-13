using CAFRI.Areas.Admin.ViewModels.Content;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class MediaController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly ContentMediaStorageService _mediaStorage;

    public MediaController(AppDbContext dbContext, ContentMediaStorageService mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? section)
    {
        ViewData["Title"] = "Media Library";
        ViewData["AdminNav"] = "media";

        var files = _mediaStorage.GetStoredImages();
        var usageMap = await BuildUsageMapAsync(files.Select(x => x.Url));

        var filteredFiles = files.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            filteredFiles = filteredFiles.Where(x =>
                x.FileName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Url.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                x.Section.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(section))
        {
            filteredFiles = filteredFiles.Where(x => string.Equals(x.Section, section.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        var allFiles = files.Count;
        var usedFiles = usageMap.Count(x => x.Value > 0);

        return View(new AdminMediaLibraryViewModel
        {
            Items = filteredFiles
                .Select(x => new AdminMediaAssetViewModel
                {
                    Url = x.Url,
                    FileName = x.FileName,
                    Section = x.Section,
                    SizeLabel = FormatSize(x.SizeBytes),
                    UpdatedLabel = x.UpdatedAtUtc.ToString("dd MMM yyyy HH:mm 'UTC'"),
                    IsUsed = usageMap.TryGetValue(x.Url, out var usageCount) && usageCount > 0,
                    UsageCount = usageMap.TryGetValue(x.Url, out usageCount) ? usageCount : 0
                })
                .ToList(),
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Files", Value = allFiles.ToString(), Caption = "Stored in uploads/content" },
                new AdminSummaryCardViewModel { Label = "In Use", Value = usedFiles.ToString(), Caption = "Referenced by CMS records" },
                new AdminSummaryCardViewModel { Label = "Unused", Value = (allFiles - usedFiles).ToString(), Caption = "Safe cleanup candidates" },
                new AdminSummaryCardViewModel { Label = "Sections", Value = files.Select(x => x.Section).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Caption = "Content buckets" }
            ],
            Search = search?.Trim() ?? string.Empty,
            Section = section?.Trim() ?? string.Empty,
            AvailableSections = files.Select(x => x.Section).Distinct(StringComparer.OrdinalIgnoreCase).Order().ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return RedirectToAction(nameof(Index));
        }

        var usageCount = await CountUsageAsync(url);
        if (usageCount > 0)
        {
            TempData["AdminError"] = "This file is still used by published or draft content and cannot be deleted yet.";
            return RedirectToAction(nameof(Index));
        }

        _mediaStorage.DeleteImage(url);
        TempData["AdminSuccess"] = "Media file deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<Dictionary<string, int>> BuildUsageMapAsync(IEnumerable<string> urls)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var url in urls.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            result[url] = await CountUsageAsync(url);
        }

        return result;
    }

    private async Task<int> CountUsageAsync(string url)
    {
        var countries = await _dbContext.CountryContents.AsNoTracking().CountAsync(x => x.HeroGalleryJson != null && x.HeroGalleryJson.Contains(url));
        var publications = await _dbContext.PublicationContentItems.AsNoTracking().CountAsync(x =>
            x.HeroImageUrl == url ||
            (x.GalleryJson != null && x.GalleryJson.Contains(url)));
        var intelligence = await _dbContext.IntelligenceContentItems.AsNoTracking().CountAsync(x => x.HeroImageUrl == url);

        return countries + publications + intelligence;
    }

    private static string FormatSize(long sizeBytes)
    {
        if (sizeBytes >= 1024 * 1024)
        {
            return $"{sizeBytes / 1024d / 1024d:0.0} MB";
        }

        if (sizeBytes >= 1024)
        {
            return $"{sizeBytes / 1024d:0.0} KB";
        }

        return $"{sizeBytes} B";
    }
}
