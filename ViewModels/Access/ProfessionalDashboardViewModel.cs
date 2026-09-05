using CAFRI.Domain.Access;

namespace CAFRI.ViewModels.Access;

public sealed class ProfessionalDashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Organization { get; set; }
    public string? Position { get; set; }
    public AccessType? AccessType { get; set; }
    public DateTimeOffset? GrantedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
    public bool IsProfessional { get; set; }
}
