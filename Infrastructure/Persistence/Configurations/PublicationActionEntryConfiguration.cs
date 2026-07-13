using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationActionEntryConfiguration : IEntityTypeConfiguration<PublicationActionEntry>
{
    public void Configure(EntityTypeBuilder<PublicationActionEntry> builder)
    {
        builder.ToTable("PublicationActionEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(160).IsRequired();
        builder.HasIndex(x => new { x.PublicationContentItemId, x.DisplayOrder });
    }
}
