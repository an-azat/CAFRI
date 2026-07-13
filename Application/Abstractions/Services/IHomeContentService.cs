using CAFRI.ViewModels.Home;

namespace CAFRI.Application.Abstractions.Services;

public interface IHomeContentService
{
    HomePageViewModel GetHomePage();
}
