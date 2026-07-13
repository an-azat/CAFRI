using CAFRI.Domain.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> builder)
    {
        builder.ToTable("UserSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PaymentProvider).HasMaxLength(100);
        builder.Property(x => x.ExternalSubscriptionId).HasMaxLength(150);
        builder.Property(x => x.ExternalCustomerId).HasMaxLength(150);

        builder.HasIndex(x => new { x.UserId, x.Status, x.EndsAtUtc });
        builder.HasIndex(x => x.ExternalSubscriptionId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.SubscriptionPlan)
            .WithMany(x => x.UserSubscriptions)
            .HasForeignKey(x => x.SubscriptionPlanId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
