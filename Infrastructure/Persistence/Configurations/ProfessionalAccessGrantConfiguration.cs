using CAFRI.Domain.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class ProfessionalAccessGrantConfiguration : IEntityTypeConfiguration<ProfessionalAccessGrant>
{
    public void Configure(EntityTypeBuilder<ProfessionalAccessGrant> builder)
    {
        builder.ToTable("ProfessionalAccessGrants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes).HasColumnType("text");

        builder.HasIndex(x => new { x.UserId, x.Status, x.ExpiresAtUtc });
        builder.HasIndex(x => x.AccessRequestId);
        builder.HasIndex(x => x.UserSubscriptionId);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AccessRequest)
            .WithMany(x => x.Grants)
            .HasForeignKey(x => x.AccessRequestId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.UserSubscription)
            .WithMany(x => x.Grants)
            .HasForeignKey(x => x.UserSubscriptionId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
