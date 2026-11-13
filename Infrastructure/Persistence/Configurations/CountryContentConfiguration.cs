using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class CountryContentConfiguration : IEntityTypeConfiguration<CountryContent>
{
    public void Configure(EntityTypeBuilder<CountryContent> builder)
    {
        builder.ToTable("CountryContents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(10).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Capital).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Gdp).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Population).HasMaxLength(100).IsRequired();
        builder.Property(x => x.BankAssets).HasMaxLength(100).IsRequired();
        builder.Property(x => x.HeroClassName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ProfileLead).HasMaxLength(600).IsRequired();
        builder.Property(x => x.OfficialName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Region).HasMaxLength(100).IsRequired();
        builder.Property(x => x.FlagImageUrl).HasMaxLength(1000);
        builder.Property(x => x.PopulationPeriod).HasMaxLength(150).IsRequired();
        builder.Property(x => x.GdpPeriod).HasMaxLength(150).IsRequired();
        builder.Property(x => x.OfficialLanguages).HasMaxLength(150).IsRequired();
        builder.Property(x => x.DataSource).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AboutText).HasColumnType("text").IsRequired();
        builder.Property(x => x.Area).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NeighbouringCountries).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProfileJson).HasColumnType("text");
        builder.Property(x => x.HeroGalleryJson).HasColumnType("text");
        builder.Property(x => x.DetailsJson).HasColumnType("text").IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsPublished, x.DisplayOrder });
    }
}
