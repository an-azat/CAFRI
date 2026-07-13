using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationContentItemConfiguration : IEntityTypeConfiguration<PublicationContentItem>
{
    public void Configure(EntityTypeBuilder<PublicationContentItem> builder)
    {
        builder.ToTable("PublicationContentItems");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CountryLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.Meta).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Topic).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PublishedDate).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ReadingTime).HasMaxLength(120).IsRequired();
        builder.Property(x => x.AuthorLabel).HasMaxLength(160).IsRequired();
        builder.Property(x => x.DocumentLabel).HasMaxLength(160).IsRequired();
        builder.Property(x => x.HeroVisualClassName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.HeroImageUrl).HasMaxLength(1000);
        builder.Property(x => x.GalleryJson).HasColumnType("text");
        builder.Property(x => x.PdfDownloadUrl).HasMaxLength(1000);
        builder.Property(x => x.DetailsJson).HasColumnType("text");
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.IsPublished, x.DisplayOrder });

        builder.HasMany(x => x.ActionEntries).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.HighlightEntries).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.FindingEntries).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Sections).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.DocumentEntries).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.RelatedLinks).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.InfoEntries).WithOne(x => x.PublicationContentItem).HasForeignKey(x => x.PublicationContentItemId).OnDelete(DeleteBehavior.Cascade);
    }
}
