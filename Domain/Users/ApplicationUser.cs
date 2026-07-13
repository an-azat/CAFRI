using Microsoft.AspNetCore.Identity;

namespace CAFRI.Domain.Users;

public sealed class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
    public string? Organization { get; set; }
    public string? Position { get; set; }
    public bool IsProfessional { get; set; }
}
