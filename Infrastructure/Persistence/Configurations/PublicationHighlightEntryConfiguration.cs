using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationHighlightEntryConfiguration : IEntityTypeConfiguration<PublicationHighlightEntry>
{
    public void Configure(EntityTypeBuilder<PublicationHighlightEntry> builder)
    {
        builder.ToTable("PublicationHighlightEntries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(600).IsRequired();
        builder.HasIndex(x => new { x.PublicationContentItemId, x.DisplayOrder });
    }
}
