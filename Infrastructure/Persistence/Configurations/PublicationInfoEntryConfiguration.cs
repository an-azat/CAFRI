using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationInfoEntryConfiguration : IEntityTypeConfiguration<PublicationInfoEntry>
{
    public void Configure(EntityTypeBuilder<PublicationInfoEntry> builder)
    {
        builder.ToTable("PublicationInfoEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(1000).IsRequired();
        builder.HasIndex(x => new { x.PublicationContentItemId, x.DisplayOrder });
    }
}
