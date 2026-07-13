using System.Security.Claims;
using CAFRI.Application.Abstractions.Services;
using Microsoft.AspNetCore.Authorization;

namespace CAFRI.Infrastructure.Authorization;

public sealed class ProfessionalAccessHandler : AuthorizationHandler<ProfessionalAccessRequirement>
{
    private readonly IProfessionalAccessService _professionalAccessService;

    public ProfessionalAccessHandler(IProfessionalAccessService professionalAccessService)
    {
        _professionalAccessService = professionalAccessService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProfessionalAccessRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (await _professionalAccessService.HasActiveAccessAsync(userId))
        {
            context.Succeed(requirement);
        }
    }
}
