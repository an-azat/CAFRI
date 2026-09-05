using CAFRI.ViewModels.Home;

namespace CAFRI.Application.Abstractions.Services;

public interface IHomeContentService
{
    Task<HomePageViewModel> GetHomePageAsync(CancellationToken cancellationToken = default);
}
