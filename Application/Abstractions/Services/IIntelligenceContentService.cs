using CAFRI.ViewModels.Intelligence;

namespace CAFRI.Application.Abstractions.Services;

public interface IIntelligenceContentService
{
    Task<IntelligencePageViewModel> GetIndexPageAsync(
        string? search = null,
        string? country = null,
        string? category = null,
        string? institution = null,
        string? dateRange = null,
        string? sortBy = null,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<IntelligenceDetailsViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default);
}
