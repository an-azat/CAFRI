using CAFRI.Domain.Users;
using CAFRI.Infrastructure.Persistence;
using CAFRI.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Controllers;

public sealed class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AppDbContext _dbContext;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        AppDbContext dbContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["Title"] = "Sign In";
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        ViewData["Title"] = "Sign In";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        ViewData["Title"] = "Access Denied";
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Activate(string token, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Activate Account";

        var activation = await _dbContext.AccountActivationTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Token == token, cancellationToken);

        if (activation is null || activation.ConsumedAtUtc != null || activation.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return View("ActivationInvalid");
        }

        return View(new ActivationViewModel { Token = token });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(ActivationViewModel model, CancellationToken cancellationToken)
    {
        ViewData["Title"] = "Activate Account";

        var activation = await _dbContext.AccountActivationTokens
            .FirstOrDefaultAsync(x => x.Token == model.Token, cancellationToken);

        if (activation is null || activation.ConsumedAtUtc != null || activation.ExpiresAtUtc <= DateTimeOffset.UtcNow)
        {
            return View("ActivationInvalid");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(activation.UserId);
        if (user is null)
        {
            return View("ActivationInvalid");
        }

        IdentityResult result;
        if (await _userManager.HasPasswordAsync(user))
        {
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            result = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);
        }
        else
        {
            result = await _userManager.AddPasswordAsync(user, model.Password);
        }

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);

        activation.ConsumedAtUtc = DateTimeOffset.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Dashboard", "Access");
    }
}
