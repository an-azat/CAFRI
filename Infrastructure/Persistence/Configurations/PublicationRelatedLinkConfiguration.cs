using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationRelatedLinkConfiguration : IEntityTypeConfiguration<PublicationRelatedLink>
{
    public void Configure(EntityTypeBuilder<PublicationRelatedLink> builder)
    {
        builder.ToTable("PublicationRelatedLinks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.LinkType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Meta).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(1000);
        builder.HasIndex(x => new { x.PublicationContentItemId, x.LinkType, x.DisplayOrder });
    }
}
