using CAFRI.ViewModels.Publications;

namespace CAFRI.Application.Abstractions.Services;

public interface IPublicationContentService
{
    PublicationsPageViewModel GetIndexPage(
        string? query = null,
        string? type = null,
        string? country = null,
        string? topic = null,
        string? tab = null,
        int page = 1);

    PublicationDetailsViewModel? GetDetails(string slug);
}
