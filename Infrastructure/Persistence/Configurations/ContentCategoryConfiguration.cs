using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class ContentCategoryConfiguration : IEntityTypeConfiguration<ContentCategory>
{
    public void Configure(EntityTypeBuilder<ContentCategory> builder)
    {
        builder.ToTable("ContentCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(180).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Scope).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.Scope, x.DisplayOrder });
    }
}
