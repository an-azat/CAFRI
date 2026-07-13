using CAFRI.Domain.Access;
using CAFRI.Domain.Users;

namespace CAFRI.Domain.Subscriptions;

public sealed class UserSubscription
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid SubscriptionPlanId { get; set; }

    public SubscriptionStatus Status { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }

    public string? PaymentProvider { get; set; }
    public string? ExternalSubscriptionId { get; set; }
    public string? ExternalCustomerId { get; set; }
    public bool AutoRenew { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
    public SubscriptionPlan SubscriptionPlan { get; set; } = null!;
    public ICollection<ProfessionalAccessGrant> Grants { get; set; } = new List<ProfessionalAccessGrant>();
}
