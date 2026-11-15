using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAFRI.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Data-only migration: hides the 12 demo Intelligence items from June 2026
    /// (IsPublished = false, nothing deleted) and adds 5 articles on monetary-policy and
    /// financial-sector news from October-November 2026 (sources are stored in each
    /// item's related links). New items use Ids starting with 'c0ffee00-2026-1100-'.
    /// Down() removes the new items and re-publishes the demo items.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20261115120000_ReplaceDemoIntelligenceWithNov2026")]
    public partial class ReplaceDemoIntelligenceWithNov2026 : Migration
    {
        private const string DemoSlugs = """
    'national-bank-updates-foreign-exchange-reporting-requirements',
    'kazakhstan-middle-corridor-rail-freight-volumes-increase-34',
    'central-bank-of-uzbekistan-raises-key-policy-rate-to-14',
    'kyrgyzstan-implements-new-customs-procedures-for-eaeu-trade',
    'kazakhstan-updates-list-of-sanctioned-entities',
    'uzbekistan-gdp-growth-reaches-6-8-in-q1-2026',
    'national-bank-of-tajikistan-issues-new-banking-license-regulations',
    'kyrgyzstan-approves-strategy-for-digital-customs-modernization',
    'turkmenistan-ratifies-adb-agreement-on-development-financing',
    'uzbekistan-strengthens-aml-cft-supervision-framework',
    'tajikistan-and-china-expand-trade-corridor-infrastructure',
    'kyrgyzstan-aligns-sanctions-regime-with-unsc-resolutions'
""";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear copies inserted by hand from documentation/dev (child rows cascade).
            migrationBuilder.Sql("""DELETE FROM "IntelligenceContentItems" WHERE "Id"::text LIKE 'c0ffee00-2026-1100-%';""");

            migrationBuilder.Sql($"""UPDATE "IntelligenceContentItems" SET "IsPublished" = false, "UpdatedAtUtc" = now() WHERE "Slug" IN ({DemoSlugs});""");

            migrationBuilder.Sql(
"""

-- ============ 1. Kazakhstan ============
INSERT INTO "IntelligenceContentItems" ("Id","Slug","Category","CountryCode","CountryName","PublishedAtUtc","Title","Description","Source","HeroImageUrl","DetailsJson","DisplayOrder","IsPublished","IsTrending","TrendingOrder","CreatedAtUtc","UpdatedAtUtc")
VALUES ('c0ffee00-2026-1100-0000-000000000001','national-bank-of-kazakhstan-cuts-base-rate-to-15-75','MACROECONOMICS','KZ','Kazakhstan','2026-10-23 07:00:00+00',
'National Bank of Kazakhstan cuts base rate to 15.75%',
'The regulator lowered the base rate by 50 bps as annual inflation slowed for a 12th consecutive month to 9.3%.',
'National Bank of Kazakhstan',NULL,NULL,0,true,false,0,now(),now());

INSERT INTO "IntelligenceArticleSections" ("Id","IntelligenceContentItemId","SectionKey","Heading","Body","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','overview','Overview','On 23 October 2026 the National Bank of Kazakhstan reduced its base rate by 50 basis points to 15.75%, with an interest-rate corridor of ±1 pp (14.75%–16.75%). The move was larger than the 25 bps cut expected by part of the market.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','overview','Overview','The decision was supported by a sustained slowdown in annual inflation to 9.3% in September and easing household inflation expectations. The regulator nevertheless intends to keep monetary conditions moderately tight and sees a low probability of another rate change before the end of 2026, warning that a base effect from regulated utility tariffs may push inflation up somewhat toward year-end.',1);

INSERT INTO "IntelligenceKeyChangeEntries" ("Id","IntelligenceContentItemId","Number","Title","Description","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001',1,'Base rate lowered by 50 bps','The base rate now stands at 15.75% with a ±1 pp corridor (14.75%–16.75%).',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001',2,'Inflation down for 12 months','Annual inflation reached 9.3% in September; food 8.5%, non-food 11.0%, services 8.9%.',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001',3,'Expectations easing','12-month household inflation expectations fell to 11.8%; market forecast for end-2026 lowered to 9.5%.',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001',4,'Cautious forward guidance','Another change before end-2026 is considered unlikely; inflation target of 5% is set for 2028.',3);

INSERT INTO "IntelligenceImpactEntries" ("Id","IntelligenceContentItemId","Title","Description","ImpactLabel","ImpactTone","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Banks','Gradual repricing of tenge loans and deposits; deposit margins may narrow.','Impact: Medium','impact-medium',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Borrowers','Slightly cheaper tenge credit for businesses and households.','Impact: Positive','impact-positive',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Investors','Signals the start of a careful easing cycle while real rates stay positive.','Impact: Medium','impact-medium',2);

INSERT INTO "IntelligenceTimelineEntries" ("Id","IntelligenceContentItemId","DateLabel","Description","Stage","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Sep 2026','Annual inflation slows to 9.3%','past',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Oct 23, 2026','Base rate cut to 15.75%','past',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Dec 4, 2026','Next scheduled base rate decision','future',2);

INSERT INTO "IntelligenceDocumentInfoEntries" ("Id","IntelligenceContentItemId","Label","Value","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Effective Date','October 27, 2026',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Document Type','Monetary policy decision',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Issuing Authority','National Bank of Kazakhstan',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Date Issued','October 23, 2026',3),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Applies To','Banking sector, borrowers and depositors in tenge',4);

INSERT INTO "IntelligenceHighlightEntries" ("Id","IntelligenceContentItemId","Text","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Base rate: 15.75% (−50 bps)',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Annual inflation: 9.3% (September)',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','GDP growth Jan–Sep 2026: 4.2%',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Next decision: December 4, 2026',3);

INSERT INTO "IntelligenceRelatedLinks" ("Id","IntelligenceContentItemId","LinkType","Title","Subtitle","Url","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','OfficialSource','National Bank of Kazakhstan press release','Base rate lowered to 15.75%','https://nationalbank.kz/en/news/press-relizy/20503',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','OfficialSource','The Astana Times','Coverage of the October 23 decision','https://astanatimes.com/2026/10/kazakhstan-cuts-base-rate-to-15-75-as-annual-inflation-falls-to-9-3/',1);

INSERT INTO "IntelligenceStatusEntries" ("Id","IntelligenceContentItemId","Label","Value","IsStatus","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Status','In Force',true,0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000001','Next Review','December 4, 2026',false,1);

-- ============ 2. Uzbekistan ============
INSERT INTO "IntelligenceContentItems" ("Id","Slug","Category","CountryCode","CountryName","PublishedAtUtc","Title","Description","Source","HeroImageUrl","DetailsJson","DisplayOrder","IsPublished","IsTrending","TrendingOrder","CreatedAtUtc","UpdatedAtUtc")
VALUES ('c0ffee00-2026-1100-0000-000000000002','central-bank-of-uzbekistan-holds-policy-rate-at-14-november-2026','BANKING','UZ','Uzbekistan','2026-11-04 09:00:00+00',
'Central Bank of Uzbekistan keeps policy rate at 14% and raises inflation outlook',
'The board left the policy rate unchanged at 14% and revised its 2026 inflation forecast upward, citing persistent supply-side cost pressures.',
'Central Bank of Uzbekistan',NULL,NULL,0,true,false,0,now(),now());

INSERT INTO "IntelligenceArticleSections" ("Id","IntelligenceContentItemId","SectionKey","Heading","Body","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','overview','Overview','At its board meeting on 4 November 2026 the Central Bank of Uzbekistan kept the policy rate at 14% per annum, where it has stood since March 2025. The regulator raised its year-end inflation forecast because of persistent supply-side cost pressures, and analysts described the accompanying message as more hawkish than in September.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','overview','Overview','In September the bank noted that annual inflation had slowed to 6.2% in August, while inflation expectations were falling more slowly than actual inflation. It continues to target 5% inflation by the end of 2027. At a press conference on 3 November the bank indicated that its 2027–2029 guidelines allow the policy rate to fall below 10% over the medium term.',1);

INSERT INTO "IntelligenceKeyChangeEntries" ("Id","IntelligenceContentItemId","Number","Title","Description","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002',1,'Policy rate unchanged at 14%','The rate has been held since March 2025.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002',2,'Inflation forecast raised','Year-end 2026 inflation outlook revised up on supply-side cost pressures.',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002',3,'Medium-term easing path','2027–2029 guidelines allow the rate to fall below 10% if disinflation continues.',2);

INSERT INTO "IntelligenceImpactEntries" ("Id","IntelligenceContentItemId","Title","Description","ImpactLabel","ImpactTone","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Banks','Funding costs remain high; lending growth continues to normalise.','Impact: Medium','impact-medium',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Borrowers','No near-term relief for soum loan rates.','Impact: High','impact-high',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Savers','Positive real rates continue to support soum deposits.','Impact: Positive','impact-positive',2);

INSERT INTO "IntelligenceTimelineEntries" ("Id","IntelligenceContentItemId","DateLabel","Description","Stage","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Sep 16, 2026','Rate held at 14%; August inflation at 6.2%','past',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Nov 4, 2026','Rate held at 14%; inflation forecast raised','current',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Dec 2026','Next board meeting on the policy rate','future',2);

INSERT INTO "IntelligenceDocumentInfoEntries" ("Id","IntelligenceContentItemId","Label","Value","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Document Type','Monetary policy decision',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Issuing Authority','Central Bank of Uzbekistan',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Date Issued','November 4, 2026',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Applies To','Banking sector and soum-denominated lending',3);

INSERT INTO "IntelligenceHighlightEntries" ("Id","IntelligenceContentItemId","Text","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Policy rate: 14% (unchanged since March 2025)',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','2026 inflation forecast revised upward',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Inflation target: 5% by end-2027',2);

INSERT INTO "IntelligenceRelatedLinks" ("Id","IntelligenceContentItemId","LinkType","Title","Subtitle","Url","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','OfficialSource','Central Bank of Uzbekistan','Press releases','https://cbu.uz/en/press_center/releases/',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','OfficialSource','AKIpress','Coverage of the November 4 decision','https://akipress.com/news:929284',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','OfficialSource','Kun.uz','Key rate could fall below 10% over next three years','https://kun.uz/en/news/2026/11/06/central-bank-uzbekistans-key-rate-could-fall-below-10-over-next-three-years-59555',2);

INSERT INTO "IntelligenceStatusEntries" ("Id","IntelligenceContentItemId","Label","Value","IsStatus","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Status','Monitoring',true,0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000002','Next Review','December 2026',false,1);

-- ============ 3. Kyrgyzstan ============
INSERT INTO "IntelligenceContentItems" ("Id","Slug","Category","CountryCode","CountryName","PublishedAtUtc","Title","Description","Source","HeroImageUrl","DetailsJson","DisplayOrder","IsPublished","IsTrending","TrendingOrder","CreatedAtUtc","UpdatedAtUtc")
VALUES ('c0ffee00-2026-1100-0000-000000000003','national-bank-of-kyrgyzstan-holds-discount-rate-at-12-october-2026','BANKING','KG','Kyrgyzstan','2026-10-27 06:00:00+00',
'National Bank of Kyrgyzstan holds discount rate at 12%',
'The board kept the discount rate at 12% as annual inflation slowed to 10.8%, pointing to persistent external inflation risks.',
'National Bank of the Kyrgyz Republic',NULL,NULL,0,true,false,0,now(),now());

INSERT INTO "IntelligenceArticleSections" ("Id","IntelligenceContentItemId","SectionKey","Heading","Body","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','overview','Overview','On 26 October 2026 the board of the National Bank of the Kyrgyz Republic decided to keep the discount rate at 12% per annum, effective 27 October. The rate was raised to 12% in February 2026 and has been held since.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','overview','Overview','Annual inflation eased to 10.8% in October from 12.2% in September, with year-to-date price growth of 8.5%. The regulator links the remaining pressure to external factors, including global food and raw-material prices, and continues to target 5–7% inflation over the medium term. Real GDP grew 10.8% in the first nine months of 2026.',1);

INSERT INTO "IntelligenceKeyChangeEntries" ("Id","IntelligenceContentItemId","Number","Title","Description","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003',1,'Discount rate held at 12%','Fourth consecutive decision without change since the February hike.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003',2,'Inflation slowing','Annual inflation down to 10.8% from 12.2% a month earlier.',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003',3,'External risks dominate','Global food and commodity prices remain the main source of inflation risk.',2);

INSERT INTO "IntelligenceImpactEntries" ("Id","IntelligenceContentItemId","Title","Description","ImpactLabel","ImpactTone","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Banks','Tight conditions keep som funding and lending rates elevated.','Impact: Medium','impact-medium',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Businesses','Borrowing costs stay high despite strong economic growth.','Impact: High','impact-high',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Households','Slower inflation supports real incomes.','Impact: Positive','impact-positive',2);

INSERT INTO "IntelligenceTimelineEntries" ("Id","IntelligenceContentItemId","DateLabel","Description","Stage","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Feb 23, 2026','Discount rate raised to 12%','past',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','May–Jul 2026','Rate held at 12%','past',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Oct 26, 2026','Rate held at 12%; effective October 27','current',2);

INSERT INTO "IntelligenceDocumentInfoEntries" ("Id","IntelligenceContentItemId","Label","Value","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Effective Date','October 27, 2026',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Document Type','Board decision on the discount rate',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Issuing Authority','National Bank of the Kyrgyz Republic',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Applies To','Banking sector and som-denominated operations',3);

INSERT INTO "IntelligenceHighlightEntries" ("Id","IntelligenceContentItemId","Text","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Discount rate: 12%',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Annual inflation: 10.8% (October)',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Real GDP growth Jan–Sep 2026: 10.8%',2);

INSERT INTO "IntelligenceRelatedLinks" ("Id","IntelligenceContentItemId","LinkType","Title","Subtitle","Url","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','OfficialSource','Open.kg','The National Bank maintained the discount rate at 12%','https://open.kg/en/news/economy/132803-nacbank-sohranil-uchetnuju-stavku-na-urovne-12.html',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','OfficialSource','National Bank of the Kyrgyz Republic','Official website','https://www.nbkr.kg/',1);

INSERT INTO "IntelligenceStatusEntries" ("Id","IntelligenceContentItemId","Label","Value","IsStatus","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000003','Status','In Force',true,0);

-- ============ 4. Tajikistan ============
INSERT INTO "IntelligenceContentItems" ("Id","Slug","Category","CountryCode","CountryName","PublishedAtUtc","Title","Description","Source","HeroImageUrl","DetailsJson","DisplayOrder","IsPublished","IsTrending","TrendingOrder","CreatedAtUtc","UpdatedAtUtc")
VALUES ('c0ffee00-2026-1100-0000-000000000004','national-bank-of-tajikistan-keeps-refinancing-rate-at-7','BANKING','TJ','Tajikistan','2026-10-29 12:00:00+00',
'National Bank of Tajikistan keeps refinancing rate at 7%',
'The rate was held for the third consecutive meeting, with annual inflation at 4.1% and inside the 5% ±2 pp target band.',
'National Bank of Tajikistan',NULL,NULL,0,true,false,0,now(),now());

INSERT INTO "IntelligenceArticleSections" ("Id","IntelligenceContentItemId","SectionKey","Heading","Body","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','overview','Overview','On 29 October 2026 the Monetary Policy Committee of the National Bank of Tajikistan left the refinancing rate unchanged at 7% per annum. It was the third hold in a row after the rate was cut from 7.5% in January, and the final scheduled meeting of 2026.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','overview','Overview','The overnight lending rate remains at 10% and the overnight deposit rate at 4%; required reserve ratios are 3% for somoni and 9% for foreign currency. Annual inflation stood at 4.1% in September, inside the 5% ±2 pp target range, although earlier statements pointed to risks from world fuel prices and volatility in imported consumer goods.',1);

INSERT INTO "IntelligenceKeyChangeEntries" ("Id","IntelligenceContentItemId","Number","Title","Description","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004',1,'Refinancing rate held at 7%','Third consecutive hold after the January cut.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004',2,'Corridor unchanged','Overnight loans 10%, overnight deposits 4%.',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004',3,'Reserve requirements unchanged','3% for national currency, 9% for foreign currency.',2);

INSERT INTO "IntelligenceImpactEntries" ("Id","IntelligenceContentItemId","Title","Description","ImpactLabel","ImpactTone","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Banks','Stable policy settings and reserve rules through year-end.','Impact: Positive','impact-positive',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Borrowers','No change in somoni credit conditions.','Impact: Medium','impact-medium',1);

INSERT INTO "IntelligenceTimelineEntries" ("Id","IntelligenceContentItemId","DateLabel","Description","Stage","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Jan 29, 2026','Rate cut from 7.5% to 7%','past',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Apr 30 & Jul 30, 2026','Rate held at 7%','past',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Oct 29, 2026','Rate held at 7%; last meeting of 2026','current',2);

INSERT INTO "IntelligenceDocumentInfoEntries" ("Id","IntelligenceContentItemId","Label","Value","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Document Type','Monetary Policy Committee decision',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Issuing Authority','National Bank of Tajikistan',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Date Issued','October 29, 2026',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Applies To','Credit institutions of Tajikistan',3);

INSERT INTO "IntelligenceHighlightEntries" ("Id","IntelligenceContentItemId","Text","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Refinancing rate: 7%',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Annual inflation: 4.1% (September)',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Target: 5% ±2 pp',2);

INSERT INTO "IntelligenceRelatedLinks" ("Id","IntelligenceContentItemId","LinkType","Title","Subtitle","Url","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','OfficialSource','National Bank of Tajikistan','News and monetary policy decisions','https://nbt.tj/en/news/',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','OfficialSource','Interfax','Coverage of the October 29 decision','https://interfax.com/newsroom/top-stories/119741/',1);

INSERT INTO "IntelligenceStatusEntries" ("Id","IntelligenceContentItemId","Label","Value","IsStatus","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000004','Status','In Force',true,0);

-- ============ 5. Turkmenistan ============
INSERT INTO "IntelligenceContentItems" ("Id","Slug","Category","CountryCode","CountryName","PublishedAtUtc","Title","Description","Source","HeroImageUrl","DetailsJson","DisplayOrder","IsPublished","IsTrending","TrendingOrder","CreatedAtUtc","UpdatedAtUtc")
VALUES ('c0ffee00-2026-1100-0000-000000000005','turkmenistan-steps-up-banking-and-financial-sector-modernization','FINANCIAL REGULATION','TM','Turkmenistan','2026-11-02 08:00:00+00',
'Turkmenistan steps up modernization of banking and financial sector',
'The President instructed the government to accelerate financial-sector reforms under the Digital Economy Development Concept for 2026–2028.',
'Government of Turkmenistan',NULL,NULL,0,true,false,0,now(),now());

INSERT INTO "IntelligenceArticleSections" ("Id","IntelligenceContentItemId","SectionKey","Heading","Body","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','overview','Overview','At a government meeting reported on 2 November 2026, President Serdar Berdimuhamedov instructed Deputy Chairman of the Cabinet of Ministers Hojamyrat Geldimyradov to step up work on modernising the financial, economic and banking sectors as part of the economic diversification strategy.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','overview','Overview','Priority areas include banking and insurance services, digital financial technologies, economic regulation and related legislation, and modernisation of national statistics, all within the Concept for the Development of the Digital Economy for 2026–2028. Card usage continues to grow: 6.7 million bank cards were registered in January–September 2026, and cashless transactions accounted for 47.6% of card-system transactions as of 1 September.',1);

INSERT INTO "IntelligenceKeyChangeEntries" ("Id","IntelligenceContentItemId","Number","Title","Description","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005',1,'Accelerated reform mandate','Government instructed to speed up modernisation of banking and financial services.',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005',2,'Digital finance focus','Fintech, digital services and insurance named as priority areas.',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005',3,'Regulatory framework update','Economic regulation mechanisms and legislation to be improved.',2);

INSERT INTO "IntelligenceImpactEntries" ("Id","IntelligenceContentItemId","Title","Description","ImpactLabel","ImpactTone","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','State banks','Infrastructure and digital-service upgrades continue under government oversight.','Impact: Medium','impact-medium',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Foreign partners','Cooperation with IFIs, including a new EBRD strategy for 2026–2030, remains a priority.','Impact: Positive','impact-positive',1);

INSERT INTO "IntelligenceTimelineEntries" ("Id","IntelligenceContentItemId","DateLabel","Description","Stage","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Jan 2026','Infrastructure upgrades approved for the Central Bank and state banks','past',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Jun 2026','IMF calls for broader financial-sector reforms','past',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Nov 2, 2026','President orders acceleration of sector modernisation','current',2),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','2026–2028','Implementation of the Digital Economy Development Concept','future',3);

INSERT INTO "IntelligenceDocumentInfoEntries" ("Id","IntelligenceContentItemId","Label","Value","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Document Type','Government policy instruction',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Legal Basis','Concept for the Development of the Digital Economy 2026–2028',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Applies To','Banking, insurance and financial-services sector',2);

INSERT INTO "IntelligenceHighlightEntries" ("Id","IntelligenceContentItemId","Text","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','6.7 million bank cards registered (Jan–Sep 2026, +4.6%)',0),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Cashless share of card transactions: 47.6%',1),
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','EBRD strategy for 2026–2030 in preparation',2);

INSERT INTO "IntelligenceRelatedLinks" ("Id","IntelligenceContentItemId","LinkType","Title","Subtitle","Url","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','OfficialSource','Trend News Agency','Turkmenistan targets modernization of financial and banking sector','https://www.trend.az/casia/turkmenistan/4241807.html',0);

INSERT INTO "IntelligenceStatusEntries" ("Id","IntelligenceContentItemId","Label","Value","IsStatus","DisplayOrder") VALUES
(gen_random_uuid(),'c0ffee00-2026-1100-0000-000000000005','Status','Monitoring',true,0);

""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DELETE FROM "IntelligenceContentItems" WHERE "Id"::text LIKE 'c0ffee00-2026-1100-%';""");

            migrationBuilder.Sql($"""UPDATE "IntelligenceContentItems" SET "IsPublished" = true, "UpdatedAtUtc" = now() WHERE "Slug" IN ({DemoSlugs});""");
        }
    }
}
