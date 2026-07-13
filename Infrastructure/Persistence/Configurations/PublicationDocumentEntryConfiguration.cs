using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationDocumentEntryConfiguration : IEntityTypeConfiguration<PublicationDocumentEntry>
{
    public void Configure(EntityTypeBuilder<PublicationDocumentEntry> builder)
    {
        builder.ToTable("PublicationDocumentEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Meta).HasMaxLength(300).IsRequired();
        builder.Property(x => x.DownloadUrl).HasMaxLength(1000);
        builder.HasIndex(x => new { x.PublicationContentItemId, x.DisplayOrder });
    }
}
