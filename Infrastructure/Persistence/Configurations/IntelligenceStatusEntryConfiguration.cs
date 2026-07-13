using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceStatusEntryConfiguration : IEntityTypeConfiguration<IntelligenceStatusEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceStatusEntry> builder)
    {
        builder.ToTable("IntelligenceStatusEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(300).IsRequired();
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
