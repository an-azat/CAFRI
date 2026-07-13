using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceArticleSectionConfiguration : IEntityTypeConfiguration<IntelligenceArticleSection>
{
    public void Configure(EntityTypeBuilder<IntelligenceArticleSection> builder)
    {
        builder.ToTable("IntelligenceArticleSections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SectionKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Heading).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Body).HasColumnType("text");
        builder.HasIndex(x => new { x.IntelligenceContentItemId, x.SectionKey, x.DisplayOrder });
    }
}
