using CAFRI.Areas.Admin.ViewModels.Content;
using CAFRI.Domain.Content;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class SourcesController : Controller
{
    private readonly AppDbContext _dbContext;

    public SourcesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? type, string? country, string? status)
    {
        ViewData["Title"] = "Admin Sources";
        ViewData["AdminNav"] = "sources";

        var query = _dbContext.ContentSources.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Summary, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(x => x.SourceType == type);
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            query = query.Where(x => x.CountryCode == country);
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
        var allItems = await _dbContext.ContentSources.AsNoTracking().ToListAsync();

        return View(new SourceIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Sources", Value = allItems.Count.ToString(), Caption = "Tracked references" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.IsPublished).ToString(), Caption = "Visible to editors" },
                new AdminSummaryCardViewModel { Label = "Types", Value = allItems.Select(x => x.SourceType).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Caption = "Source classes" },
                new AdminSummaryCardViewModel { Label = "Coverage", Value = allItems.Select(x => x.CountryCode).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Caption = "Country / regional buckets" }
            ],
            AvailableTypes = allItems.Select(x => x.SourceType).Distinct().Order().ToList(),
            AvailableCountries = allItems.Select(x => x.CountryCode).Distinct().Order().ToList(),
            Search = search,
            Type = type,
            Country = country,
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Source";
        ViewData["AdminNav"] = "sources";
        return View("Edit", new ContentSourceEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContentSourceEditViewModel model)
    {
        if (!ModelState.IsValid) return View("Edit", model);

        _dbContext.ContentSources.Add(new ContentSource
        {
            Id = Guid.NewGuid(),
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            SourceType = model.SourceType.Trim(),
            CountryCode = model.CountryCode.Trim(),
            Url = model.Url.Trim(),
            Summary = model.Summary.Trim(),
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
        var entity = await _dbContext.ContentSources.FindAsync(id);
        if (entity is null) return NotFound();

        ViewData["Title"] = "Edit Source";
        ViewData["AdminNav"] = "sources";
        return View(new ContentSourceEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            SourceType = entity.SourceType,
            CountryCode = entity.CountryCode,
            Url = entity.Url,
            Summary = entity.Summary,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ContentSourceEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = await _dbContext.ContentSources.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Name = model.Name.Trim();
        entity.Slug = model.Slug.Trim();
        entity.SourceType = model.SourceType.Trim();
        entity.CountryCode = model.CountryCode.Trim();
        entity.Url = model.Url.Trim();
        entity.Summary = model.Summary.Trim();
        entity.DisplayOrder = model.DisplayOrder;
        entity.IsPublished = model.IsPublished;
        entity.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var entity = await _dbContext.ContentSources.FindAsync(id);
        if (entity is null) return NotFound();
        _dbContext.ContentSources.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
