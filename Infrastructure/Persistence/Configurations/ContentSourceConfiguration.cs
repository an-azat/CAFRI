using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class ContentSourceConfiguration : IEntityTypeConfiguration<ContentSource>
{
    public void Configure(EntityTypeBuilder<ContentSource> builder)
    {
        builder.ToTable("ContentSources");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(220).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.SourceType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1200).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.CountryCode, x.SourceType, x.DisplayOrder });
    }
}
