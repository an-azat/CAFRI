using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class PublicationCountryAssignmentConfiguration : IEntityTypeConfiguration<PublicationCountryAssignment>
{
    public void Configure(EntityTypeBuilder<PublicationCountryAssignment> builder)
    {
        builder.ToTable("PublicationCountryAssignments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CountryCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.CountryLabel).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => new { x.PublicationContentItemId, x.CountryCode }).IsUnique();
    }
}
