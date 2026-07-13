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
public sealed class IndicatorsController : Controller
{
    private readonly AppDbContext _dbContext;

    public IndicatorsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? category, string? country, string? status)
    {
        ViewData["Title"] = "Admin Indicators";
        ViewData["AdminNav"] = "indicators";

        var query = _dbContext.CountryIndicators.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Notes, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(x => x.Category == category);
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
        var allItems = await _dbContext.CountryIndicators.AsNoTracking().ToListAsync();

        return View(new IndicatorIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Indicators", Value = allItems.Count.ToString(), Caption = "Tracked data points" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.IsPublished).ToString(), Caption = "Available for use" },
                new AdminSummaryCardViewModel { Label = "Categories", Value = allItems.Select(x => x.Category).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Caption = "Indicator groups" },
                new AdminSummaryCardViewModel { Label = "Countries", Value = allItems.Select(x => x.CountryCode).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Caption = "Coverage set" }
            ],
            AvailableCategories = allItems.Select(x => x.Category).Distinct().Order().ToList(),
            AvailableCountries = allItems.Select(x => x.CountryCode).Distinct().Order().ToList(),
            Search = search,
            Category = category,
            Country = country,
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Indicator";
        ViewData["AdminNav"] = "indicators";
        return View("Edit", new CountryIndicatorEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CountryIndicatorEditViewModel model)
    {
        if (!ModelState.IsValid) return View("Edit", model);

        _dbContext.CountryIndicators.Add(new CountryIndicator
        {
            Id = Guid.NewGuid(),
            CountryCode = model.CountryCode.Trim(),
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            Category = model.Category.Trim(),
            Unit = model.Unit.Trim(),
            LatestValue = model.LatestValue.Trim(),
            YearLabel = model.YearLabel.Trim(),
            SourceName = model.SourceName.Trim(),
            Notes = model.Notes.Trim(),
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
        var entity = await _dbContext.CountryIndicators.FindAsync(id);
        if (entity is null) return NotFound();

        ViewData["Title"] = "Edit Indicator";
        ViewData["AdminNav"] = "indicators";
        return View(new CountryIndicatorEditViewModel
        {
            Id = entity.Id,
            CountryCode = entity.CountryCode,
            Name = entity.Name,
            Slug = entity.Slug,
            Category = entity.Category,
            Unit = entity.Unit,
            LatestValue = entity.LatestValue,
            YearLabel = entity.YearLabel,
            SourceName = entity.SourceName,
            Notes = entity.Notes,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CountryIndicatorEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = await _dbContext.CountryIndicators.FindAsync(id);
        if (entity is null) return NotFound();

        entity.CountryCode = model.CountryCode.Trim();
        entity.Name = model.Name.Trim();
        entity.Slug = model.Slug.Trim();
        entity.Category = model.Category.Trim();
        entity.Unit = model.Unit.Trim();
        entity.LatestValue = model.LatestValue.Trim();
        entity.YearLabel = model.YearLabel.Trim();
        entity.SourceName = model.SourceName.Trim();
        entity.Notes = model.Notes.Trim();
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
        var entity = await _dbContext.CountryIndicators.FindAsync(id);
        if (entity is null) return NotFound();
        _dbContext.CountryIndicators.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
