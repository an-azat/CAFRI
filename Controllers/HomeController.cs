using System.Diagnostics;
using CAFRI.Application.Abstractions.Services;
using CAFRI.Models;
using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Controllers;

public sealed class HomeController : Controller
{
    private readonly IHomeContentService _homeContentService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IHomeContentService homeContentService,
        ILogger<HomeController> logger)
    {
        _homeContentService = homeContentService;
        _logger = logger;
    }

    public IActionResult Index() => View(_homeContentService.GetHomePage());

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
