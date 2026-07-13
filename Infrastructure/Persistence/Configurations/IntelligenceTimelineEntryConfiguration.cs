using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceTimelineEntryConfiguration : IEntityTypeConfiguration<IntelligenceTimelineEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceTimelineEntry> builder)
    {
        builder.ToTable("IntelligenceTimelineEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.DateLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Stage).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
