using CAFRI.Domain.Access;

namespace CAFRI.Application.Abstractions.Services;

public interface IProfessionalAccessService
{
    Task<bool> HasActiveAccessAsync(string userId, CancellationToken cancellationToken = default);

    // Signed in and either an admin or holding an active, unexpired, unrevoked access grant.
    Task<bool> CanReadProtectedContentAsync(System.Security.Claims.ClaimsPrincipal user, CancellationToken cancellationToken = default);
    Task<ProfessionalAccessGrant?> GetActiveGrantAsync(string userId, CancellationToken cancellationToken = default);
}
