using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationFindingEntryConfiguration : IEntityTypeConfiguration<PublicationFindingEntry>
{
    public void Configure(EntityTypeBuilder<PublicationFindingEntry> builder)
    {
        builder.ToTable("PublicationFindingEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Icon).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.PublicationContentItemId, x.DisplayOrder });
    }
}
