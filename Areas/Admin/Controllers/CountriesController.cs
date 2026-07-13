using CAFRI.Areas.Admin.ViewModels.Content;
using CAFRI.Domain.Content;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class CountriesController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly ContentMediaStorageService _mediaStorage;

    public CountriesController(AppDbContext dbContext, ContentMediaStorageService mediaStorage)
    {
        _dbContext = dbContext;
        _mediaStorage = mediaStorage;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status)
    {
        ViewData["Title"] = "Admin Countries";
        ViewData["AdminNav"] = "countries";

        var query = _dbContext.CountryContents.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Capital, $"%{term}%") ||
                EF.Functions.ILike(x.Code, $"%{term}%"));
        }

        if (status == "published")
        {
            query = query.Where(x => x.IsPublished);
        }
        else if (status == "draft")
        {
            query = query.Where(x => !x.IsPublished);
        }

        var items = await query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name).ToListAsync();
        var allItems = await _dbContext.CountryContents.AsNoTracking().ToListAsync();

        return View(new CountryIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Countries", Value = allItems.Count.ToString(), Caption = "Profiles in CMS" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.IsPublished).ToString(), Caption = "Visible on the platform" },
                new AdminSummaryCardViewModel { Label = "Drafts", Value = allItems.Count(x => !x.IsPublished).ToString(), Caption = "Awaiting release" },
                new AdminSummaryCardViewModel { Label = "Region Coverage", Value = "Central Asia", Caption = "Current focus region" }
            ],
            Search = search,
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Country";
        ViewData["AdminNav"] = "countries";
        return View("Edit", BuildEditViewModel(new CountryContentEditViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CountryContentEditViewModel model)
    {
        var galleryFiles = Request.Form.Files.GetFiles(nameof(model.HeroGalleryFiles));
        if (galleryFiles.Any(file => !_mediaStorage.IsSupportedImage(file)))
        {
            ModelState.AddModelError(nameof(model.HeroGalleryFiles), $"One or more gallery files use an unsupported format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }

        if (!ModelState.IsValid) return View("Edit", BuildEditViewModel(model));

        var galleryItems = ContentImageTextSerializer.Parse(model.HeroGalleryText).ToList();
        galleryItems.AddRange(await _mediaStorage.SaveImagesAsync(galleryFiles, "countries", HttpContext.RequestAborted));

        _dbContext.CountryContents.Add(new CountryContent
        {
            Id = Guid.NewGuid(),
            Code = model.Code.Trim(),
            Slug = model.Slug.Trim(),
            Name = model.Name.Trim(),
            Capital = model.Capital.Trim(),
            Summary = model.Summary.Trim(),
            Gdp = model.Gdp.Trim(),
            Population = model.Population.Trim(),
            BankAssets = model.BankAssets.Trim(),
            HeroClassName = model.HeroClassName.Trim(),
            HeroGalleryJson = ContentImageTextSerializer.Serialize(galleryItems),
            DetailsJson = model.DetailsJson.Trim(),
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        });
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.CountryContents.FindAsync(id);
        if (entity is null) return NotFound();

        ViewData["Title"] = "Edit Country";
        ViewData["AdminNav"] = "countries";
        return View(new CountryContentEditViewModel
        {
            Id = entity.Id,
            Code = entity.Code,
            Slug = entity.Slug,
            Name = entity.Name,
            Capital = entity.Capital,
            Summary = entity.Summary,
            Gdp = entity.Gdp,
            Population = entity.Population,
            BankAssets = entity.BankAssets,
            HeroClassName = entity.HeroClassName,
            HeroGalleryText = BuildGalleryText(entity.HeroGalleryJson),
            DetailsJson = entity.DetailsJson,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CountryContentEditViewModel model)
    {
        var galleryFiles = Request.Form.Files.GetFiles(nameof(model.HeroGalleryFiles));
        if (galleryFiles.Any(file => !_mediaStorage.IsSupportedImage(file)))
        {
            ModelState.AddModelError(nameof(model.HeroGalleryFiles), $"One or more gallery files use an unsupported format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }

        if (!ModelState.IsValid) return View(BuildEditViewModel(model));
        var entity = await _dbContext.CountryContents.FindAsync(id);
        if (entity is null) return NotFound();

        var previousUrls = ContentImageTextSerializer.Parse(entity.HeroGalleryJson)
            .Select(x => x.ImageUrl)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var galleryItems = ContentImageTextSerializer.Parse(model.HeroGalleryText).ToList();
        galleryItems.AddRange(await _mediaStorage.SaveImagesAsync(galleryFiles, "countries", HttpContext.RequestAborted));
        var currentUrls = galleryItems
            .Select(x => x.ImageUrl)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        entity.Code = model.Code.Trim();
        entity.Slug = model.Slug.Trim();
        entity.Name = model.Name.Trim();
        entity.Capital = model.Capital.Trim();
        entity.Summary = model.Summary.Trim();
        entity.Gdp = model.Gdp.Trim();
        entity.Population = model.Population.Trim();
        entity.BankAssets = model.BankAssets.Trim();
        entity.HeroClassName = model.HeroClassName.Trim();
        entity.HeroGalleryJson = ContentImageTextSerializer.Serialize(galleryItems);
        entity.DetailsJson = model.DetailsJson.Trim();
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsPublished = model.IsPublished;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(previousUrls.Except(currentUrls, StringComparer.OrdinalIgnoreCase));
        return RedirectToAction(nameof(Index));
    }

    private static string BuildGalleryText(string? raw) =>
        ContentImageTextSerializer.Serialize(ContentImageTextSerializer.Parse(raw));

    private static CountryContentEditViewModel BuildEditViewModel(CountryContentEditViewModel model) =>
        new()
        {
            Id = model.Id,
            Code = model.Code,
            Slug = model.Slug,
            Name = model.Name,
            Capital = model.Capital,
            Summary = model.Summary,
            Gdp = model.Gdp,
            Population = model.Population,
            BankAssets = model.BankAssets,
            HeroClassName = model.HeroClassName,
            HeroGalleryText = model.HeroGalleryText,
            DetailsJson = model.DetailsJson,
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished
        };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _dbContext.CountryContents.FindAsync(id);
        if (entity is null) return NotFound();
        var deletedUrls = ContentImageTextSerializer.Parse(entity.HeroGalleryJson)
            .Select(x => x.ImageUrl)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        _dbContext.CountryContents.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(deletedUrls);
        return RedirectToAction(nameof(Index));
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
}
