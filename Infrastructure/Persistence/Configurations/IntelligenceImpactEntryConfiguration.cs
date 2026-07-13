using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceImpactEntryConfiguration : IEntityTypeConfiguration<IntelligenceImpactEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceImpactEntry> builder)
    {
        builder.ToTable("IntelligenceImpactEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.ImpactLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ImpactTone).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
