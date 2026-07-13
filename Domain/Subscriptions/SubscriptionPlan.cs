namespace CAFRI.Domain.Subscriptions;

public sealed class SubscriptionPlan
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int DurationDays { get; set; }

    public bool IsActive { get; set; }
    public bool IsProfessionalAccessIncluded { get; set; } = true;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
