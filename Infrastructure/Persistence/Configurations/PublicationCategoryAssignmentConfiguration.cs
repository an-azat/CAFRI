using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationCategoryAssignmentConfiguration : IEntityTypeConfiguration<PublicationCategoryAssignment>
{
    public void Configure(EntityTypeBuilder<PublicationCategoryAssignment> builder)
    {
        builder.ToTable("PublicationCategoryAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CategoryName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.CategorySlug).HasMaxLength(180).IsRequired();
        builder.HasIndex(x => new { x.PublicationContentItemId, x.CategorySlug }).IsUnique();
    }
}
