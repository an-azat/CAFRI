using System.Security.Claims;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Domain.Access;
using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Services.Access;

public sealed class ProfessionalAccessService : IProfessionalAccessService
{
    private readonly AppDbContext _dbContext;

    public ProfessionalAccessService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> HasActiveAccessAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return _dbContext.ProfessionalAccessGrants.AnyAsync(
            grant => grant.UserId == userId
                && grant.Status == AccessGrantStatus.Active
                && (grant.StartsAtUtc == null || grant.StartsAtUtc <= now)
                && (grant.ExpiresAtUtc == null || grant.ExpiresAtUtc > now),
            cancellationToken);
    }

    public async Task<bool> CanReadProtectedContentAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        if (user.IsInRole(SystemRoles.Admin))
        {
            return true;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return !string.IsNullOrEmpty(userId) && await HasActiveAccessAsync(userId, cancellationToken);
    }

    public Task<ProfessionalAccessGrant?> GetActiveGrantAsync(string userId, CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        return _dbContext.ProfessionalAccessGrants
            .AsNoTracking()
            .Where(grant =>
                grant.UserId == userId
                && grant.Status == AccessGrantStatus.Active
                && (grant.StartsAtUtc == null || grant.StartsAtUtc <= now)
                && (grant.ExpiresAtUtc == null || grant.ExpiresAtUtc > now))
            .OrderByDescending(grant => grant.GrantedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
