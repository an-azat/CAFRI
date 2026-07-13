using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceHighlightEntryConfiguration : IEntityTypeConfiguration<IntelligenceHighlightEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceHighlightEntry> builder)
    {
        builder.ToTable("IntelligenceHighlightEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(600).IsRequired();
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
