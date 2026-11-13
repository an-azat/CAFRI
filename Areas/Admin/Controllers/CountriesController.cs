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
        return View("Edit", new CountryContentEditViewModel { Profile = WithBlankRows(new CountryProfileContent()) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CountryContentEditViewModel model)
    {
        var galleryFiles = Request.Form.Files.GetFiles(nameof(model.HeroGalleryFiles));
        var flagFile = Request.Form.Files.GetFile(nameof(model.FlagImageFile));
        ValidateUploads(model, galleryFiles, flagFile);

        if (!ModelState.IsValid) return View("Edit", await BuildEditViewModelAsync(model));

        var galleryItems = ContentImageTextSerializer.Parse(model.HeroGalleryText).ToList();
        galleryItems.AddRange(await _mediaStorage.SaveImagesAsync(galleryFiles, "countries", HttpContext.RequestAborted));

        var entity = new CountryContent
        {
            Id = Guid.NewGuid(),
            Code = model.Code.Trim(),
            Slug = model.Slug.Trim(),
            Name = model.Name.Trim(),
            Capital = model.Capital.Trim(),
            Summary = model.Summary.Trim(),
            Gdp = model.Gdp.Trim(),
            Population = model.Population.Trim(),
            BankAssets = model.BankAssets?.Trim() ?? string.Empty,
            HeroClassName = model.HeroClassName?.Trim() ?? string.Empty,
            Currency = model.Currency?.Trim() ?? string.Empty,
            ProfileLead = model.ProfileLead?.Trim() ?? string.Empty,
            HeroGalleryJson = ContentImageTextSerializer.Serialize(galleryItems),
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished || model.SubmitAction == "publish",
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
        ApplyProfileHeaderFields(entity, model, await SaveFlagAsync(flagFile, model.FlagImageUrl));

        _dbContext.CountryContents.Add(entity);
        await _dbContext.SaveChangesAsync();
        return this.RedirectToAdminList();
    }

    private void ValidateUploads(CountryContentEditViewModel model, IReadOnlyList<IFormFile> galleryFiles, IFormFile? flagFile)
    {
        if (galleryFiles.Any(file => !_mediaStorage.IsSupportedImage(file)))
        {
            ModelState.AddModelError(nameof(model.HeroGalleryFiles), $"One or more gallery files use an unsupported format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }

        if (flagFile is { Length: > 0 } && !_mediaStorage.IsSupportedImage(flagFile))
        {
            ModelState.AddModelError(nameof(model.FlagImageFile), $"Unsupported flag image format. Allowed: {_mediaStorage.GetAllowedExtensionsLabel()}");
        }
    }

    // An uploaded flag wins over the URL field; an empty URL clears the flag.
    private async Task<string?> SaveFlagAsync(IFormFile? flagFile, string? flagUrl)
    {
        var uploaded = flagFile is { Length: > 0 }
            ? await _mediaStorage.SaveImageAsync(flagFile, "countries", HttpContext.RequestAborted)
            : null;
        return uploaded ?? (string.IsNullOrWhiteSpace(flagUrl) ? null : flagUrl.Trim());
    }

    private static void ApplyProfileHeaderFields(CountryContent entity, CountryContentEditViewModel model, string? flagImageUrl)
    {
        entity.OfficialName = model.OfficialName?.Trim() ?? string.Empty;
        entity.Region = model.Region?.Trim() ?? string.Empty;
        entity.FlagImageUrl = flagImageUrl;
        entity.PopulationPeriod = model.PopulationPeriod?.Trim() ?? string.Empty;
        entity.GdpPeriod = model.GdpPeriod?.Trim() ?? string.Empty;
        entity.OfficialLanguages = model.OfficialLanguages?.Trim() ?? string.Empty;
        entity.DataSource = model.DataSource?.Trim() ?? string.Empty;
        entity.AboutText = model.AboutText?.Trim() ?? string.Empty;
        entity.Area = model.Area?.Trim() ?? string.Empty;
        entity.NeighbouringCountries = model.NeighbouringCountries?.Trim() ?? string.Empty;
        entity.ProfileJson = CountryProfileContentSerializer.Serialize(model.Profile);
    }

    // The admin form renders every list with a few blank rows to add entries; the serializer
    // drops whatever is still blank on save.
    private static CountryProfileContent WithBlankRows(CountryProfileContent profile)
    {
        static void Pad<T>(List<T> rows, int extra, int minimum) where T : new()
        {
            var target = Math.Max(rows.Count + extra, minimum);
            while (rows.Count < target)
            {
                rows.Add(new T());
            }
        }

        // Every standard snapshot card gets a row with its label ready to fill in.
        foreach (var standard in CountryProfileStandards.SnapshotIndicators.Where(s => !profile.Kpis.Any(k => s.Matches(k.Label))))
        {
            profile.Kpis.Add(new CountryProfileKpi { Label = standard.Label });
        }

        Pad(profile.Kpis, 1, 4);
        Pad(profile.GdpHistory, 2, 6);
        Pad(profile.Signals, 1, 3);
        Pad(profile.FiscalRows, 1, 3);
        Pad(profile.AnalysisPoints, 1, 3);
        Pad(profile.Priorities, 1, 4);
        Pad(profile.News, 1, 4);
        Pad(profile.Sources, 2, 4);
        return profile;
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _dbContext.CountryContents.FindAsync(id);
        if (entity is null) return NotFound();

        ViewData["Title"] = "Edit Country";
        ViewData["AdminNav"] = "countries";
        return View(await BuildEditViewModelAsync(new CountryContentEditViewModel
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
            Currency = entity.Currency,
            ProfileLead = entity.ProfileLead,
            OfficialName = entity.OfficialName,
            Region = entity.Region,
            FlagImageUrl = entity.FlagImageUrl,
            PopulationPeriod = entity.PopulationPeriod,
            GdpPeriod = entity.GdpPeriod,
            OfficialLanguages = entity.OfficialLanguages,
            DataSource = entity.DataSource,
            AboutText = entity.AboutText,
            Area = entity.Area,
            NeighbouringCountries = entity.NeighbouringCountries,
            Profile = CountryProfileContentSerializer.Parse(entity.ProfileJson),
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CountryContentEditViewModel model)
    {
        var galleryFiles = Request.Form.Files.GetFiles(nameof(model.HeroGalleryFiles));
        var flagFile = Request.Form.Files.GetFile(nameof(model.FlagImageFile));
        ValidateUploads(model, galleryFiles, flagFile);

        if (!ModelState.IsValid) return View(await BuildEditViewModelAsync(model));
        var entity = await _dbContext.CountryContents.FindAsync(id);
        if (entity is null) return NotFound();

        var previousUrls = ContentImageTextSerializer.Parse(entity.HeroGalleryJson)
            .Select(x => x.ImageUrl)
            .Append(entity.FlagImageUrl ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var galleryItems = ContentImageTextSerializer.Parse(model.HeroGalleryText).ToList();
        galleryItems.AddRange(await _mediaStorage.SaveImagesAsync(galleryFiles, "countries", HttpContext.RequestAborted));
        var flagImageUrl = await SaveFlagAsync(flagFile, model.FlagImageUrl);
        var currentUrls = galleryItems
            .Select(x => x.ImageUrl)
            .Append(flagImageUrl ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        entity.Code = model.Code.Trim();
        entity.Slug = model.Slug.Trim();
        entity.Name = model.Name.Trim();
        entity.Capital = model.Capital.Trim();
        entity.Summary = model.Summary.Trim();
        entity.Gdp = model.Gdp.Trim();
        entity.Population = model.Population.Trim();
        entity.BankAssets = model.BankAssets?.Trim() ?? string.Empty;
        entity.HeroClassName = model.HeroClassName?.Trim() ?? string.Empty;
        entity.Currency = model.Currency?.Trim() ?? string.Empty;
        entity.ProfileLead = model.ProfileLead?.Trim() ?? string.Empty;
        ApplyProfileHeaderFields(entity, model, flagImageUrl);
        entity.HeroGalleryJson = ContentImageTextSerializer.Serialize(galleryItems);
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsPublished = model.IsPublished || model.SubmitAction == "publish";
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(previousUrls.Except(currentUrls, StringComparer.OrdinalIgnoreCase));
        return this.RedirectToAdminList();
    }

    private static string BuildGalleryText(string? raw) =>
        ContentImageTextSerializer.Serialize(ContentImageTextSerializer.Parse(raw));

    private async Task<CountryContentEditViewModel> BuildEditViewModelAsync(CountryContentEditViewModel model)
    {
        var code = model.Code?.Trim() ?? string.Empty;
        var indicators = await _dbContext.CountryIndicators
            .AsNoTracking()
            .Where(x => x.CountryCode == code)
            .OrderBy(x => x.Name)
            .ThenByDescending(x => x.YearLabel)
            .Select(x => new CountryLinkedRowViewModel
            {
                Title = x.Name,
                Detail = (x.YearLabel + ": " + x.LatestValue + " " + x.Unit).Trim(),
                IsPublished = x.IsPublished
            })
            .ToListAsync();
        var sources = await _dbContext.ContentSources
            .AsNoTracking()
            .Where(x => x.CountryCode == code)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .Select(x => new CountryLinkedRowViewModel { Title = x.Name, Detail = x.SourceType, IsPublished = x.IsPublished })
            .ToListAsync();

        return new CountryContentEditViewModel
        {
            Id = model.Id,
            Code = model.Code ?? string.Empty,
            Slug = model.Slug,
            Name = model.Name,
            Capital = model.Capital,
            Summary = model.Summary,
            Gdp = model.Gdp,
            Population = model.Population,
            BankAssets = model.BankAssets,
            HeroClassName = model.HeroClassName,
            HeroGalleryText = model.HeroGalleryText,
            Currency = model.Currency,
            ProfileLead = model.ProfileLead,
            OfficialName = model.OfficialName,
            Region = model.Region,
            FlagImageUrl = model.FlagImageUrl,
            PopulationPeriod = model.PopulationPeriod,
            GdpPeriod = model.GdpPeriod,
            OfficialLanguages = model.OfficialLanguages,
            DataSource = model.DataSource,
            AboutText = model.AboutText,
            Area = model.Area,
            NeighbouringCountries = model.NeighbouringCountries,
            Profile = WithBlankRows(model.Profile),
            DisplayOrder = model.DisplayOrder,
            IsPublished = model.IsPublished,
            CreatedAtUtc = model.CreatedAtUtc,
            UpdatedAtUtc = model.UpdatedAtUtc,
            Indicators = indicators,
            Sources = sources
        };
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _dbContext.CountryContents.FindAsync(id);
        if (entity is null) return NotFound();
        var deletedUrls = ContentImageTextSerializer.Parse(entity.HeroGalleryJson)
            .Select(x => x.ImageUrl)
            .Append(entity.FlagImageUrl ?? string.Empty)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();
        _dbContext.CountryContents.Remove(entity);
        await _dbContext.SaveChangesAsync();
        await DeleteUnusedMediaAsync(deletedUrls);
        return this.RedirectToAdminList();
    }

    private async Task DeleteUnusedMediaAsync(IEnumerable<string> urls)
    {
        foreach (var url in urls.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var stillUsed = await _dbContext.CountryContents.AsNoTracking().AnyAsync(x => (x.HeroGalleryJson != null && x.HeroGalleryJson.Contains(url)) || x.FlagImageUrl == url) ||
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
}
