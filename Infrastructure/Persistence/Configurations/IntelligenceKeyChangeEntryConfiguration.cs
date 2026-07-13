using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceKeyChangeEntryConfiguration : IEntityTypeConfiguration<IntelligenceKeyChangeEntry>
{
    public void Configure(EntityTypeBuilder<IntelligenceKeyChangeEntry> builder)
    {
        builder.ToTable("IntelligenceKeyChangeEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.DisplayOrder });
    }
}
