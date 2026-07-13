using CAFRI.Domain.Subscriptions;
using CAFRI.Domain.Users;

namespace CAFRI.Domain.Access;

public sealed class ProfessionalAccessGrant
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;

    public AccessType AccessType { get; set; }
    public AccessGrantStatus Status { get; set; }

    public DateTimeOffset GrantedAtUtc { get; set; }
    public DateTimeOffset? StartsAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }

    public string? GrantedByUserId { get; set; }
    public string? RevokedByUserId { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? Notes { get; set; }

    public Guid? AccessRequestId { get; set; }
    public Guid? UserSubscriptionId { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public AccessRequest? AccessRequest { get; set; }
    public UserSubscription? UserSubscription { get; set; }
}
