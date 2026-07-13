using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class CountryIndicatorConfiguration : IEntityTypeConfiguration<CountryIndicator>
{
    public void Configure(EntityTypeBuilder<CountryIndicator> builder)
    {
        builder.ToTable("CountryIndicators");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(40).IsRequired();
        builder.Property(x => x.LatestValue).HasMaxLength(120).IsRequired();
        builder.Property(x => x.YearLabel).HasMaxLength(60).IsRequired();
        builder.Property(x => x.SourceName).HasMaxLength(220).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.CountryCode, x.Category, x.DisplayOrder });
    }
}
