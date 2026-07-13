using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceRelatedLinkConfiguration : IEntityTypeConfiguration<IntelligenceRelatedLink>
{
    public void Configure(EntityTypeBuilder<IntelligenceRelatedLink> builder)
    {
        builder.ToTable("IntelligenceRelatedLinks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LinkType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Subtitle).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1000);
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.LinkType, x.DisplayOrder });
    }
}
