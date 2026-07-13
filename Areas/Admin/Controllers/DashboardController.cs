using CAFRI.Areas.Admin.ViewModels.Dashboard;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class DashboardController : Controller
{
    private readonly AppDbContext _dbContext;

    public DashboardController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewData["Title"] = "CAFRI Admin Panel";
        ViewData["AdminNav"] = "dashboard";

        var now = DateTimeOffset.UtcNow;
        var monthStart = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var publicationsCount = await _dbContext.PublicationContentItems.CountAsync(cancellationToken);
        var intelligenceCount = await _dbContext.IntelligenceContentItems.CountAsync(cancellationToken);
        var countriesCount = await _dbContext.CountryContents.CountAsync(cancellationToken);
        var pendingReviewCount = await _dbContext.AccessRequests.CountAsync(x => x.Status == Domain.Access.AccessRequestStatus.Pending, cancellationToken);
        var professionalUsersCount = await _dbContext.Users.CountAsync(x => x.IsProfessional, cancellationToken);
        var activeSubscriptionsCount = await _dbContext.UserSubscriptions.CountAsync(cancellationToken);
        var sourcesCount = await _dbContext.ContentSources.CountAsync(cancellationToken);
        var indicatorsCount = await _dbContext.CountryIndicators.CountAsync(cancellationToken);

        var recentPublications = (await _dbContext.PublicationContentItems
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(4)
            .ToListAsync(cancellationToken))
            .Select(x => new AdminRecentContentItemViewModel
            {
                Title = x.Title,
                ContentType = "Publication",
                CountryOrCoverage = x.CountryLabel,
                TopicOrCategory = x.Type,
                Status = x.IsPublished ? "Published" : "Draft",
                UpdatedLabel = x.UpdatedAtUtc.UtcDateTime.ToString("dd MMM yyyy"),
                UpdatedAtUtc = x.UpdatedAtUtc,
                EditUrl = Url.Action("Edit", "Publications", new { area = "Admin", id = x.Id }),
                PreviewUrl = Url.Action("Details", "Publications", new { area = "", slug = x.Slug })
            })
            .ToList();

        var recentIntelligence = (await _dbContext.IntelligenceContentItems
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(4)
            .ToListAsync(cancellationToken))
            .Select(x => new AdminRecentContentItemViewModel
            {
                Title = x.Title,
                ContentType = "Intelligence",
                CountryOrCoverage = x.CountryName,
                TopicOrCategory = x.Category,
                Status = x.IsPublished ? "Published" : "Draft",
                UpdatedLabel = x.UpdatedAtUtc.UtcDateTime.ToString("dd MMM yyyy"),
                UpdatedAtUtc = x.UpdatedAtUtc,
                EditUrl = Url.Action("Edit", "Intelligence", new { area = "Admin", id = x.Id }),
                PreviewUrl = Url.Action("Details", "Intelligence", new { area = "", slug = x.Slug })
            })
            .ToList();

        var recentCountries = (await _dbContext.CountryContents
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(3)
            .ToListAsync(cancellationToken))
            .Select(x => new AdminRecentContentItemViewModel
            {
                Title = x.Name,
                ContentType = "Country Profile",
                CountryOrCoverage = x.Capital,
                TopicOrCategory = x.Code,
                Status = x.IsPublished ? "Published" : "Draft",
                UpdatedLabel = x.UpdatedAtUtc.UtcDateTime.ToString("dd MMM yyyy"),
                UpdatedAtUtc = x.UpdatedAtUtc,
                EditUrl = Url.Action("Edit", "Countries", new { area = "Admin", id = x.Id }),
                PreviewUrl = Url.Action("Details", "Countries", new { area = "", slug = x.Slug })
            })
            .ToList();

        var recentContent = recentPublications
            .Concat(recentIntelligence)
            .Concat(recentCountries)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .Take(8)
            .ToList();

        var model = new AdminDashboardViewModel
        {
            Metrics =
            [
                new AdminMetricCardViewModel { Label = "Publications", Value = publicationsCount.ToString(), ChangeLabel = $"+{await _dbContext.PublicationContentItems.CountAsync(x => x.CreatedAtUtc >= monthStart, cancellationToken)} this month", Accent = "blue" },
                new AdminMetricCardViewModel { Label = "Intelligence Items", Value = intelligenceCount.ToString(), ChangeLabel = $"+{await _dbContext.IntelligenceContentItems.CountAsync(x => x.CreatedAtUtc >= monthStart, cancellationToken)} this month", Accent = "cyan" },
                new AdminMetricCardViewModel { Label = "Countries", Value = countriesCount.ToString(), ChangeLabel = $"{await _dbContext.CountryContents.CountAsync(x => x.IsPublished, cancellationToken)} published", Accent = "violet" },
                new AdminMetricCardViewModel { Label = "Pending Review", Value = pendingReviewCount.ToString(), ChangeLabel = "Access queue", Accent = "amber" },
                new AdminMetricCardViewModel { Label = "Professional Users", Value = professionalUsersCount.ToString(), ChangeLabel = "Approved accounts", Accent = "emerald" },
                new AdminMetricCardViewModel { Label = "Sources", Value = sourcesCount.ToString(), ChangeLabel = "Reference library", Accent = "slate" },
                new AdminMetricCardViewModel { Label = "Indicators", Value = indicatorsCount.ToString(), ChangeLabel = "Data points tracked", Accent = "violet" },
                new AdminMetricCardViewModel { Label = "Subscriptions", Value = activeSubscriptionsCount.ToString(), ChangeLabel = "Future billing layer", Accent = "slate" }
            ],
            RecentContent = recentContent
        };

        return View(model);
    }
}
