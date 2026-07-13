using CAFRI.Application.Abstractions.Services;
using CAFRI.ViewModels.Home;

namespace CAFRI.Infrastructure.Content;

public sealed class JsonHomeContentService : IHomeContentService
{
    private readonly Lazy<HomePageViewModel> _page;

    public JsonHomeContentService(JsonContentFileLoader loader)
    {
        _page = new Lazy<HomePageViewModel>(() => loader.Load<HomeContentDocument>("App_Data/content/home.json").Page);
    }

    public HomePageViewModel GetHomePage() => _page.Value;
}
