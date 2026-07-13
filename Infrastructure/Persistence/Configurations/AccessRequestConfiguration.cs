using CAFRI.Domain.Access;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.ToTable("AccessRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Organization).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Position).HasMaxLength(150).IsRequired();
        builder.Property(x => x.BusinessEmail).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Country).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PurposeOfAccess).HasColumnType("text").IsRequired();
        builder.Property(x => x.ReviewNote).HasColumnType("text");

        builder.HasIndex(x => x.BusinessEmail);
        builder.HasIndex(x => new { x.Status, x.CreatedAtUtc });

        builder.HasOne(x => x.ApplicationUser)
            .WithMany()
            .HasForeignKey(x => x.ApplicationUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
