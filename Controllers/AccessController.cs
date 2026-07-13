using CAFRI.Domain.Access;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Access;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CAFRI.Controllers;

public sealed class AccessController : Controller
{
    private readonly AppDbContext _dbContext;

    public AccessController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ActionName("Request")]
    public IActionResult RequestAccess()
    {
        ViewData["Title"] = "Professional Access";
        return View(new AccessRequestViewModel());
    }

    [HttpPost]
    [ActionName("Request")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitRequest(AccessRequestViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Professional Access";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var accessRequest = new AccessRequest
        {
            Id = Guid.NewGuid(),
            FullName = model.FullName.Trim(),
            Organization = model.Organization.Trim(),
            Position = model.Position.Trim(),
            BusinessEmail = model.BusinessEmail.Trim(),
            Country = model.Country.Trim(),
            PurposeOfAccess = model.PurposeOfAccess.Trim(),
            Status = AccessRequestStatus.Pending,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        _dbContext.AccessRequests.Add(accessRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return RedirectToAction(nameof(Confirmation), new { email = accessRequest.BusinessEmail });
    }

    [HttpGet]
    public IActionResult Confirmation(string? email)
    {
        ViewData["Title"] = "Request Received";
        ViewBag.Email = email;
        return View();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, cancellationToken);

        if (user is null)
        {
            return Challenge();
        }

        ViewData["Title"] = "Account Dashboard";

        return View(new ProfessionalDashboardViewModel
        {
            FullName = user.FullName ?? user.Email ?? "CAFRI User",
            Email = user.Email ?? string.Empty,
            Organization = user.Organization,
            Position = user.Position,
            AccessType = AccessType.Approval,
            GrantedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = null,
            IsProfessional = User.IsInRole("Admin")
        });
    }
}
