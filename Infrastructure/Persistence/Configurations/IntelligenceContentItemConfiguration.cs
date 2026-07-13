using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class IntelligenceContentItemConfiguration : IEntityTypeConfiguration<IntelligenceContentItem>
{
    public void Configure(EntityTypeBuilder<IntelligenceContentItem> builder)
    {
        builder.ToTable("IntelligenceContentItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CountryName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PublishedLabel).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(300).IsRequired();
        builder.Property(x => x.HeroImageUrl).HasMaxLength(1000);
        builder.Property(x => x.DetailsJson).HasColumnType("text");
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.IsPublished, x.DisplayOrder });

        builder.HasMany(x => x.Sections).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.KeyChangeEntries).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.ImpactEntries).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.TimelineEntries).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.DocumentInfoEntries).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.OfficialDocuments).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.RelatedLinks).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.StatusEntries).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.HighlightEntries).WithOne(x => x.IntelligenceContentItem).HasForeignKey(x => x.IntelligenceContentItemId).OnDelete(DeleteBehavior.Cascade);
    }
}
