using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationArticleSectionConfiguration : IEntityTypeConfiguration<PublicationArticleSection>
{
    public void Configure(EntityTypeBuilder<PublicationArticleSection> builder)
    {
        builder.ToTable("PublicationArticleSections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SectionKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Heading).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ParagraphsText).HasColumnType("text").IsRequired();
        builder.Property(x => x.BulletPointsText).HasColumnType("text").IsRequired();
        builder.Property(x => x.CalloutTone).HasMaxLength(40);
        builder.Property(x => x.CalloutLabel).HasMaxLength(120);
        builder.Property(x => x.CalloutTitle).HasMaxLength(200);
        builder.Property(x => x.CalloutDescription).HasColumnType("text");
        builder.HasIndex(x => new { x.PublicationContentItemId, x.SectionKey, x.DisplayOrder });
    }
}
