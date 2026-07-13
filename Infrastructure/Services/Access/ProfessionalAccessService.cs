using CAFRI.Application.Abstractions.Services;
using CAFRI.Domain.Access;
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
