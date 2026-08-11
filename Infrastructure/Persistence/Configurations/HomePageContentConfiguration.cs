using CAFRI.Domain.Content;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAFRI.Infrastructure.Persistence.Configurations;

public sealed class HomePageContentConfiguration : IEntityTypeConfiguration<HomePageContent>
{
    public void Configure(EntityTypeBuilder<HomePageContent> builder)
    {
        builder.ToTable("HomePageContents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.HeroTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.HeroLead).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.HeroPrimaryCtaLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.HeroPrimaryCtaUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.HeroSecondaryCtaLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.HeroSecondaryCtaUrl).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IntroTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.IntroDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.PlatformSectionLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PlatformSectionTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.PlatformSectionDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.DirectionsText).HasColumnType("text").IsRequired();
        builder.Property(x => x.CountriesSectionLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CountriesSectionTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.CountriesSectionDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.CountriesVisualTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.CountriesVisualDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.FeaturesSectionLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.FeaturesSectionTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.FeaturesSectionDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.FeatureCardsText).HasColumnType("text").IsRequired();
        builder.Property(x => x.LatestSectionLabel).HasMaxLength(120).IsRequired();
        builder.Property(x => x.LatestSectionTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.LatestSectionDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.FeaturedPublicationEyebrow).HasMaxLength(160).IsRequired();
        builder.Property(x => x.LatestFeaturedLinkLabel).HasMaxLength(160).IsRequired();
        builder.Property(x => x.LatestIntelligenceLinkLabel).HasMaxLength(160).IsRequired();
        builder.Property(x => x.AboutTitle).HasMaxLength(300).IsRequired();
        builder.Property(x => x.AboutDescription).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.CoverageAreasText).HasColumnType("text").IsRequired();
        builder.HasIndex(x => x.FeaturedPublicationId);
    }
}
