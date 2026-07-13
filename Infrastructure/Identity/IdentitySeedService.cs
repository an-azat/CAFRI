using CAFRI.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CAFRI.Infrastructure.Identity;

public sealed class IdentitySeedService
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly AdminSeedOptions _options;

    public IdentitySeedService(
        RoleManager<IdentityRole> roleManager,
        UserManager<ApplicationUser> userManager,
        IOptions<AdminSeedOptions> options)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _options = options.Value;
    }

    public async Task SeedAsync()
    {
        await EnsureRoleAsync(SystemRoles.Admin);
        await EnsureRoleAsync(SystemRoles.Professional);

        var adminUser = await _userManager.FindByEmailAsync(_options.Email);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = _options.Email,
                Email = _options.Email,
                EmailConfirmed = true,
                FullName = _options.FullName,
                IsProfessional = true
            };

            var createResult = await _userManager.CreateAsync(adminUser, _options.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed admin user: {errors}");
            }
        }
        else
        {
            adminUser.FullName = _options.FullName;
            adminUser.IsProfessional = true;
            await _userManager.UpdateAsync(adminUser);
        }

        await EnsureUserInRoleAsync(adminUser, SystemRoles.Admin);
        await EnsureUserInRoleAsync(adminUser, SystemRoles.Professional);
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to seed role '{roleName}': {errors}");
            }
        }
    }

    private async Task EnsureUserInRoleAsync(ApplicationUser user, string roleName)
    {
        if (!await _userManager.IsInRoleAsync(user, roleName))
        {
            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"Failed to add user '{user.Email}' to role '{roleName}': {errors}");
            }
        }
    }
}
