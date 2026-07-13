using CAFRI.ViewModels.Intelligence;

namespace CAFRI.Application.Abstractions.Services;

public interface IIntelligenceContentService
{
    IntelligencePageViewModel GetIndexPage(
        string? search = null,
        string? country = null,
        string? category = null,
        string? institution = null,
        string? dateRange = null,
        string? sortBy = null,
        int page = 1);

    IntelligenceDetailsViewModel? GetDetails(string slug);
}
