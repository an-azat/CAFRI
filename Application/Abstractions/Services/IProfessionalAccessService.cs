using CAFRI.Domain.Access;

namespace CAFRI.Application.Abstractions.Services;

public interface IProfessionalAccessService
{
    Task<bool> HasActiveAccessAsync(string userId, CancellationToken cancellationToken = default);
    Task<ProfessionalAccessGrant?> GetActiveGrantAsync(string userId, CancellationToken cancellationToken = default);
}
