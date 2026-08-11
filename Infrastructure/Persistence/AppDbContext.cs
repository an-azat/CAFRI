using CAFRI.Domain.Access;
using CAFRI.Domain.Content;
using CAFRI.Domain.Subscriptions;
using CAFRI.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CAFRI.Infrastructure.Persistence;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<AccountActivationToken> AccountActivationTokens => Set<AccountActivationToken>();
    public DbSet<ProfessionalAccessGrant> ProfessionalAccessGrants => Set<ProfessionalAccessGrant>();
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();
    public DbSet<CountryContent> CountryContents => Set<CountryContent>();
    public DbSet<HomePageContent> HomePageContents => Set<HomePageContent>();
    public DbSet<IntelligenceContentItem> IntelligenceContentItems => Set<IntelligenceContentItem>();
    public DbSet<PublicationContentItem> PublicationContentItems => Set<PublicationContentItem>();
    public DbSet<PublicationCountryAssignment> PublicationCountryAssignments => Set<PublicationCountryAssignment>();
    public DbSet<PublicationCategoryAssignment> PublicationCategoryAssignments => Set<PublicationCategoryAssignment>();
    public DbSet<ContentCategory> ContentCategories => Set<ContentCategory>();
    public DbSet<ContentSource> ContentSources => Set<ContentSource>();
    public DbSet<CountryIndicator> CountryIndicators => Set<CountryIndicator>();
    public DbSet<IntelligenceArticleSection> IntelligenceArticleSections => Set<IntelligenceArticleSection>();
    public DbSet<IntelligenceKeyChangeEntry> IntelligenceKeyChangeEntries => Set<IntelligenceKeyChangeEntry>();
    public DbSet<IntelligenceImpactEntry> IntelligenceImpactEntries => Set<IntelligenceImpactEntry>();
    public DbSet<IntelligenceTimelineEntry> IntelligenceTimelineEntries => Set<IntelligenceTimelineEntry>();
    public DbSet<IntelligenceDocumentInfoEntry> IntelligenceDocumentInfoEntries => Set<IntelligenceDocumentInfoEntry>();
    public DbSet<IntelligenceOfficialDocumentEntry> IntelligenceOfficialDocumentEntries => Set<IntelligenceOfficialDocumentEntry>();
    public DbSet<IntelligenceRelatedLink> IntelligenceRelatedLinks => Set<IntelligenceRelatedLink>();
    public DbSet<IntelligenceStatusEntry> IntelligenceStatusEntries => Set<IntelligenceStatusEntry>();
    public DbSet<IntelligenceHighlightEntry> IntelligenceHighlightEntries => Set<IntelligenceHighlightEntry>();
    public DbSet<PublicationActionEntry> PublicationActionEntries => Set<PublicationActionEntry>();
    public DbSet<PublicationHighlightEntry> PublicationHighlightEntries => Set<PublicationHighlightEntry>();
    public DbSet<PublicationFindingEntry> PublicationFindingEntries => Set<PublicationFindingEntry>();
    public DbSet<PublicationArticleSection> PublicationArticleSections => Set<PublicationArticleSection>();
    public DbSet<PublicationDocumentEntry> PublicationDocumentEntries => Set<PublicationDocumentEntry>();
    public DbSet<PublicationRelatedLink> PublicationRelatedLinks => Set<PublicationRelatedLink>();
    public DbSet<PublicationInfoEntry> PublicationInfoEntries => Set<PublicationInfoEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
