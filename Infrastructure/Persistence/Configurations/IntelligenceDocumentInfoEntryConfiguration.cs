using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceDocumentInfoEntryConfiguration : IEntityTypeConfiguration<IntelligenceDocumentInfoEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceDocumentInfoEntry> builder)
    {
        builder.ToTable("IntelligenceDocumentInfoEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
