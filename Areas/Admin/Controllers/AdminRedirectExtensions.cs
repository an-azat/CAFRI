using Microsoft.AspNetCore.Mvc;

namespace CAFRI.Areas.Admin.Controllers;

public static class AdminRedirectExtensions
{
    // Back to the admin list the action was started from, keeping its filters and page.
    // The area must be explicit: Publications, Intelligence and Countries also exist as public
    // attribute-routed controllers, and an area-less RedirectToAction("Index") resolves to
    // the public site page (e.g. /publications) instead of /Admin/Publications.
    public static IActionResult RedirectToAdminList(this Controller controller)
    {
        var controllerName = controller.ControllerContext.ActionDescriptor.ControllerName;
        var listPath = $"/Admin/{controllerName}";
        var referer = controller.Request.Headers.Referer.ToString();

        if (Uri.TryCreate(referer, UriKind.Absolute, out var refererUri) &&
            string.Equals(refererUri.Host, controller.Request.Host.Host, StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(refererUri.AbsolutePath.TrimEnd('/'), listPath, StringComparison.OrdinalIgnoreCase) ||
             string.Equals(refererUri.AbsolutePath.TrimEnd('/'), $"{listPath}/Index", StringComparison.OrdinalIgnoreCase)))
        {
            return controller.LocalRedirect(refererUri.PathAndQuery);
        }

        return controller.RedirectToAction("Index", controllerName, new { area = "Admin" });
    }
}
