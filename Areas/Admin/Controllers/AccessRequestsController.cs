using CAFRI.Areas.Admin.ViewModels.AccessRequests;
using CAFRI.Domain.Access;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = SystemRoles.Admin)]
public sealed class AccessRequestsController : Controller
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccessRequestsController(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    private async Task IssueActivationTokenAsync(
        AccessRequest request,
        string userId,
        CancellationToken cancellationToken)
    {
        var previousTokens = await _dbContext.AccountActivationTokens
            .Where(x => x.AccessRequestId == request.Id && x.ConsumedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var previousToken in previousTokens)
        {
            previousToken.ConsumedAtUtc = DateTimeOffset.UtcNow;
        }

        _dbContext.AccountActivationTokens.Add(new AccountActivationToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            AccessRequestId = request.Id,
            Token = Guid.NewGuid().ToString("N"),
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Access Requests";
        ViewData["AdminNav"] = "access";

        var query = _dbContext.AccessRequests.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x =>
                EF.Functions.ILike(x.FullName, $"%{term}%") ||
                EF.Functions.ILike(x.Organization, $"%{term}%") ||
                EF.Functions.ILike(x.BusinessEmail, $"%{term}%"));
        }

        if (Enum.TryParse<AccessRequestStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        var items = await query
            .OrderBy(x => x.Status)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new AccessRequestReviewViewModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Organization = x.Organization,
                Position = x.Position,
                BusinessEmail = x.BusinessEmail,
                Country = x.Country,
                PurposeOfAccess = x.PurposeOfAccess,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc,
                ReviewedAtUtc = x.ReviewedAtUtc,
                ReviewNote = x.ReviewNote,
                HasGrant = x.Grants.Any(),
                GrantStatus = x.Grants
                    .OrderByDescending(grant => grant.GrantedAtUtc)
                    .Select(grant => (AccessGrantStatus?)grant.Status)
                    .FirstOrDefault(),
                GrantAccessType = x.Grants
                    .OrderByDescending(grant => grant.GrantedAtUtc)
                    .Select(grant => (AccessType?)grant.AccessType)
                    .FirstOrDefault(),
                GrantExpiresAtUtc = x.Grants
                    .OrderByDescending(grant => grant.GrantedAtUtc)
                    .Select(grant => grant.ExpiresAtUtc)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var allRequests = await _dbContext.AccessRequests.AsNoTracking().ToListAsync(cancellationToken);

        return View(new AccessRequestIndexViewModel
        {
            Items = items,
            SummaryCards =
            [
                new AdminAccessSummaryCardViewModel { Label = "All Requests", Value = allRequests.Count.ToString(), Caption = "Historical applications" },
                new AdminAccessSummaryCardViewModel { Label = "Pending", Value = allRequests.Count(x => x.Status == AccessRequestStatus.Pending).ToString(), Caption = "Awaiting review" },
                new AdminAccessSummaryCardViewModel { Label = "Approved", Value = allRequests.Count(x => x.Status == AccessRequestStatus.Approved).ToString(), Caption = "Professional access granted" },
                new AdminAccessSummaryCardViewModel { Label = "Declined", Value = allRequests.Count(x => x.Status == AccessRequestStatus.Declined).ToString(), Caption = "Closed requests" }
            ],
            Search = search,
            Status = status
        });
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Access Request Details";
        ViewData["AdminNav"] = "access";

        var item = await _dbContext.AccessRequests
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AccessRequestReviewViewModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Organization = x.Organization,
                Position = x.Position,
                BusinessEmail = x.BusinessEmail,
                Country = x.Country,
                PurposeOfAccess = x.PurposeOfAccess,
                Status = x.Status,
                CreatedAtUtc = x.CreatedAtUtc,
                ReviewedAtUtc = x.ReviewedAtUtc,
                ReviewNote = x.ReviewNote,
                HasGrant = x.Grants.Any(),
                GrantStatus = x.Grants
                    .OrderByDescending(grant => grant.GrantedAtUtc)
                    .Select(grant => (AccessGrantStatus?)grant.Status)
                    .FirstOrDefault(),
                GrantAccessType = x.Grants
                    .OrderByDescending(grant => grant.GrantedAtUtc)
                    .Select(grant => (AccessType?)grant.AccessType)
                    .FirstOrDefault(),
                GrantExpiresAtUtc = x.Grants
                    .OrderByDescending(grant => grant.GrantedAtUtc)
                    .Select(grant => grant.ExpiresAtUtc)
                    .FirstOrDefault(),
                ActivationToken = _dbContext.AccountActivationTokens
                    .Where(token => token.AccessRequestId == x.Id && token.ConsumedAtUtc == null)
                    .OrderByDescending(token => token.CreatedAtUtc)
                    .Select(token => token.Token)
                    .FirstOrDefault(),
                ActivationExpiresAtUtc = _dbContext.AccountActivationTokens
                    .Where(token => token.AccessRequestId == x.Id && token.ConsumedAtUtc == null)
                    .OrderByDescending(token => token.CreatedAtUtc)
                    .Select(token => (DateTimeOffset?)token.ExpiresAtUtc)
                    .FirstOrDefault(),
                ActivationStatus = _dbContext.AccountActivationTokens
                    .Where(token => token.AccessRequestId == x.Id)
                    .OrderByDescending(token => token.CreatedAtUtc)
                    .Select(token => token.ConsumedAtUtc != null
                        ? "Consumed"
                        : token.ExpiresAtUtc <= DateTimeOffset.UtcNow
                            ? "Expired"
                            : "Active")
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.AccessRequests
            .Include(x => x.Grants)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        if (request.Status == AccessRequestStatus.Approved && request.Grants.Any())
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        var user = await _userManager.FindByEmailAsync(request.BusinessEmail);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = request.BusinessEmail,
                Email = request.BusinessEmail,
                FullName = request.FullName,
                Organization = request.Organization,
                Position = request.Position,
                EmailConfirmed = false,
                IsProfessional = true
            };

            var createResult = await _userManager.CreateAsync(user, $"Cafri!{Guid.NewGuid():N}aA1!");
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return await Details(id, cancellationToken);
            }
        }
        else
        {
            user.FullName = request.FullName;
            user.Organization = request.Organization;
            user.Position = request.Position;
            user.IsProfessional = true;
            await _userManager.UpdateAsync(user);
        }

        if (!await _userManager.IsInRoleAsync(user, SystemRoles.Professional))
        {
            await _userManager.AddToRoleAsync(user, SystemRoles.Professional);
        }

        request.ApplicationUserId = user.Id;
        request.Status = AccessRequestStatus.Approved;
        request.ReviewedAtUtc = DateTimeOffset.UtcNow;
        request.ReviewedByUserId ??= "system-admin-review";
        request.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (!request.Grants.Any())
        {
            _dbContext.ProfessionalAccessGrants.Add(new ProfessionalAccessGrant
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                AccessType = AccessType.Approval,
                Status = AccessGrantStatus.Active,
                GrantedAtUtc = DateTimeOffset.UtcNow,
                StartsAtUtc = DateTimeOffset.UtcNow,
                AccessRequestId = request.Id,
                GrantedByUserId = "system-admin-review",
                Notes = "Approved through the CAFRI admin access request workflow."
            });
        }

        await IssueActivationTokenAsync(request, user.Id, cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
        TempData["AdminSuccess"] = "Access approved and activation link issued.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegenerateActivationLink(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.AccessRequests
            .Include(x => x.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        var user = request.ApplicationUser;
        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(request.BusinessEmail);
        }

        if (user is null)
        {
            TempData["AdminError"] = "Activation link cannot be generated because no account is attached to this request yet.";
            return RedirectToAction(nameof(Details), new { id });
        }

        await IssueActivationTokenAsync(request, user.Id, cancellationToken);
        request.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        TempData["AdminSuccess"] = "Activation link regenerated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Decline(Guid id, string? reviewNote, CancellationToken cancellationToken)
    {
        var request = await _dbContext.AccessRequests.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        request.Status = AccessRequestStatus.Declined;
        request.ReviewedAtUtc = DateTimeOffset.UtcNow;
        request.ReviewNote = string.IsNullOrWhiteSpace(reviewNote)
            ? "Declined during admin review."
            : reviewNote.Trim();
        request.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendGrant(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.AccessRequests
            .Include(x => x.Grants)
            .Include(x => x.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        var grant = request.Grants.OrderByDescending(x => x.GrantedAtUtc).FirstOrDefault();
        if (grant is null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        grant.Status = AccessGrantStatus.Suspended;
        grant.Notes = "Grant suspended by admin review workflow.";
        request.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (request.ApplicationUser is not null)
        {
            request.ApplicationUser.IsProfessional = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivateGrant(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.AccessRequests
            .Include(x => x.Grants)
            .Include(x => x.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        var grant = request.Grants.OrderByDescending(x => x.GrantedAtUtc).FirstOrDefault();
        if (grant is null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        grant.Status = AccessGrantStatus.Active;
        grant.StartsAtUtc ??= DateTimeOffset.UtcNow;
        grant.Notes = "Grant reactivated by admin review workflow.";
        request.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (request.ApplicationUser is not null)
        {
            request.ApplicationUser.IsProfessional = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevokeGrant(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.AccessRequests
            .Include(x => x.Grants)
            .Include(x => x.ApplicationUser)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (request is null)
        {
            return NotFound();
        }

        var grant = request.Grants.OrderByDescending(x => x.GrantedAtUtc).FirstOrDefault();
        if (grant is null)
        {
            return RedirectToAction(nameof(Details), new { id });
        }

        grant.Status = AccessGrantStatus.Revoked;
        grant.RevokedAtUtc = DateTimeOffset.UtcNow;
        grant.RevokedByUserId = "system-admin-review";
        grant.ExpiresAtUtc = DateTimeOffset.UtcNow;
        grant.Notes = "Grant revoked by admin review workflow.";
        request.UpdatedAtUtc = DateTimeOffset.UtcNow;

        if (request.ApplicationUser is not null)
        {
            request.ApplicationUser.IsProfessional = false;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id });
    }
}
