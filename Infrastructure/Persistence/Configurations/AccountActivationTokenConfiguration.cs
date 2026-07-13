using CAFRI.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class AccountActivationTokenConfiguration : IEntityTypeConfiguration<AccountActivationToken>
{
    public void Configure(EntityTypeBuilder<AccountActivationToken> builder)
    {
        builder.ToTable("AccountActivationTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .HasMaxLength(128)
            .IsRequired();

        builder.HasIndex(x => x.Token).IsUnique();
        builder.HasIndex(x => new { x.UserId, x.ExpiresAtUtc });

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
