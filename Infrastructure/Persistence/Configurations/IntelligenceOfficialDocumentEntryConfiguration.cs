using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceOfficialDocumentEntryConfiguration : IEntityTypeConfiguration<IntelligenceOfficialDocumentEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceOfficialDocumentEntry> builder)
    {
        builder.ToTable("IntelligenceOfficialDocumentEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Subtitle).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Meta).HasMaxLength(250).IsRequired();
        builder.Property(x => x.DownloadUrl).HasMaxLength(1000);
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
