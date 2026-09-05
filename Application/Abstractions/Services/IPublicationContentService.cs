using CAFRI.ViewModels.Publications;

namespace CAFRI.Application.Abstractions.Services;

public interface IPublicationContentService
{
    Task<PublicationsPageViewModel> GetIndexPageAsync(
        string? query = null,
        string? type = null,
        string? country = null,
        string? topic = null,
        string? tab = null,
        int page = 1,
        CancellationToken cancellationToken = default);

    Task<PublicationDetailsViewModel?> GetDetailsAsync(string slug, CancellationToken cancellationToken = default);
}
