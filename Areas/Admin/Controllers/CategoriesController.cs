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
public sealed class CategoriesController : Controller
{
    private readonly AppDbContext _dbContext;

    public CategoriesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? scope, string? status)
    {
        ViewData["Title"] = "Admin Categories";
        ViewData["AdminNav"] = "categories";

        var query = _dbContext.ContentCategories.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, $"%{term}%") ||
                EF.Functions.ILike(x.Description, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            query = query.Where(x => x.Scope == scope);
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
        var allItems = await _dbContext.ContentCategories.AsNoTracking().ToListAsync();

        return View(new CategoryIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminSummaryCardViewModel { Label = "All Categories", Value = allItems.Count.ToString(), Caption = "Taxonomy records" },
                new AdminSummaryCardViewModel { Label = "Published", Value = allItems.Count(x => x.IsPublished).ToString(), Caption = "Available to editors" },
                new AdminSummaryCardViewModel { Label = "Drafts", Value = allItems.Count(x => !x.IsPublished).ToString(), Caption = "Not yet active" },
                new AdminSummaryCardViewModel { Label = "Scopes", Value = allItems.Select(x => x.Scope).Distinct(StringComparer.OrdinalIgnoreCase).Count().ToString(), Caption = "Coverage domains" }
            ],
            Search = search,
            Scope = scope,
            Status = status
        });
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Category";
        ViewData["AdminNav"] = "categories";
        return View("Edit", new ContentCategoryEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ContentCategoryEditViewModel model)
    {
        if (!ModelState.IsValid) return View("Edit", model);

        _dbContext.ContentCategories.Add(new ContentCategory
        {
            Id = Guid.NewGuid(),
            Name = model.Name.Trim(),
            Slug = model.Slug.Trim(),
            Description = model.Description.Trim(),
            Scope = model.Scope.Trim(),
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
        var entity = await _dbContext.ContentCategories.FindAsync(id);
        if (entity is null) return NotFound();

        ViewData["Title"] = "Edit Category";
        ViewData["AdminNav"] = "categories";
        return View(new ContentCategoryEditViewModel
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug,
            Description = entity.Description,
            Scope = entity.Scope,
            DisplayOrder = entity.DisplayOrder,
            IsPublished = entity.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, ContentCategoryEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var entity = await _dbContext.ContentCategories.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Name = model.Name.Trim();
        entity.Slug = model.Slug.Trim();
        entity.Description = model.Description.Trim();
        entity.Scope = model.Scope.Trim();
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
        var entity = await _dbContext.ContentCategories.FindAsync(id);
        if (entity is null) return NotFound();
        _dbContext.ContentCategories.Remove(entity);
        await _dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
