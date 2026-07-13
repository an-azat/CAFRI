# CAFRI FINAL PRODUCT BLUEPRINT
## Central Asia Financial & Regulatory Intelligence Initiative

**Version:** 1.0 (Implementation Ready)  
**Document Status:** Approved for Development  
**Classification:** Internal Product Document  

---

## 1. PRODUCT DEFINITION

**Product Name:** CAFRI  
**Full Name:** Central Asia Financial & Regulatory Intelligence Initiative  
**Product Type:** Institutional AI-Assisted Intelligence Platform  
**Geographic Scope:** Kazakhstan, Kyrgyzstan, Uzbekistan, Tajikistan, Turkmenistan  

**Product Statement:**  
CAFRI is a professional intelligence platform providing structured, reliable, and actionable intelligence on financial regulation, banking, trade, logistics, sanctions, economic policy, and geopolitical developments across Central Asia. It is not a news website, not a media outlet, and not a blog. It is an institutional intelligence platform designed for professional decision-makers.

---

## 2. STRATEGIC POSITIONING

**Primary Positioning:** Professional institutional intelligence — comparable in presentation standard to Bloomberg Intelligence, Moody's Analytics, and Oxford Analytica.

**What CAFRI is:**
- Structured regulatory and financial intelligence
- Country-specific analytical content
- AI-assisted intelligence processing
- Professional-grade research publications
- Real-time regional overview with institutional depth

**What CAFRI is not:**
- A news website
- A media outlet
- A blog
- A data aggregator without analysis
- A startup product

---

## 3. TARGET USERS

**Primary Users:**
- International banks and financial institutions
- Investment firms operating in or evaluating Central Asia
- Government agencies monitoring regional developments
- International organizations (UN, World Bank, ADB, EBRD, IMF)
- Corporate compliance departments
- Trade and logistics companies
- Consulting firms specializing in Central Asia
- Academic and policy researchers

**User Needs (by type):**

| User Type | Primary Need |
|-----------|-------------|
| Compliance Officer | Regulatory change monitoring |
| Banking Analyst | Central bank policy tracking |
| Investment Analyst | Macroeconomic & risk intelligence |
| Trade Professional | Trade corridor & customs updates |
| Government Official | Regional policy developments |
| Researcher | Structured data and publications |

---

## 4. CORE DESIGN PRINCIPLES

1. **Every page answers one practical question.** No page exists without a purpose.
2. **Trust before beauty.** Credibility is communicated through structure and accuracy, not aesthetics.
3. **Information density over whitespace.** Professional users read, not browse.
4. **No fake data.** No invented statistics, ratings, or analytics.
5. **Official sources are primary.** AI assists; it does not replace source documents.
6. **Public value before Professional upsell.** Users receive meaningful content before being invited to upgrade.

---

## 5. INFORMATION ARCHITECTURE

### 5.1 Navigation Structure

```
CAFRI
├── Home
├── Intelligence
│   └── [Intelligence Detail Page]
├── Countries
│   ├── Kazakhstan
│   ├── Kyrgyzstan
│   ├── Uzbekistan
│   ├── Tajikistan
│   └── Turkmenistan
├── Interactive Map
├── Publications
│   └── [Publication Detail Page]
├── About
├── Search [Results Page]
└── Professional Access
    ├── Access Request Form
    ├── Confirmation Page
    └── [User Dashboard — authenticated]
```

### 5.2 URL Structure

All URLs prefixed by language code. Default: /en/

```
/en/                                         Home
/en/intelligence/                            Intelligence Feed
/en/intelligence/[slug]/                     Intelligence Detail
/en/countries/                               Countries Overview
/en/countries/kazakhstan/                    Country Profile
/en/countries/kyrgyzstan/
/en/countries/uzbekistan/
/en/countries/tajikistan/
/en/countries/turkmenistan/
/en/map/                                     Interactive Map
/en/publications/                            Publications Library
/en/publications/[slug]/                     Publication Detail
/en/about/                                   About CAFRI
/en/access/                                  Professional Access
/en/access/request/                          Request Form
/en/access/confirmation/                     Post-submission page
/en/access/dashboard/                        Authenticated user area
/en/search/?q=[query]                        Search Results
/ru/[mirrors all /en/ paths]
```

### 5.3 Core Data Objects

**COUNTRY**
```
id, name, iso_code, flag_svg, slug
overview_text, economic_summary, banking_summary
regulatory_summary, trade_summary
last_updated, map_geometry_geojson
→ has many: IntelligenceItems, Publications, Institutions, Regulations
```

**INTELLIGENCE ITEM**
```
id, title, slug, country_id, category, institution_id
publication_date, effective_date, summary
official_source_name, official_source_url
original_language, ai_executive_summary
tags[], status [draft|published|archived]
visibility [public|professional]
→ linked to: Regulation, Publication, IntelligenceItems (related)
```

**PUBLICATION**
```
id, title, slug, publication_type, country_id, author
publication_date, reading_time_minutes
executive_summary, main_content_html
references[], download_pdf_url
visibility [public|professional]
→ linked to: IntelligenceItems, Regulations
```

**INSTITUTION**
```
id, name, country_id, institution_type
description, official_website
→ linked to: Regulations, IntelligenceItems, Publications
```

**REGULATION**
```
id, title, country_id, issuing_institution_id
effective_date, status [active|superseded|draft]
category, summary, official_link
→ linked to: IntelligenceItems, Publications, Institutions
```

**SOURCE**
```
id, name, country_id, organization
source_type, website_url, rss_url
language, reliability_level, last_checked
```

**TAG**
```
id, name, slug, category, language
count (computed), related_tag_ids[]
```

### 5.4 Object Relationships

```
Country
  └── Intelligence Items  ←→  Institutions
  └── Publications        ←→  Regulations
  └── Sources             ←→  Tags
```

Search indexes: all objects across all types. Query returns grouped results.

---

## 6. PAGE SPECIFICATIONS SUMMARY

### Page Inventory

| Page | Public Access | Professional Access |
|------|--------------|---------------------|
| Home | Full | Full (enhanced) |
| Intelligence Feed | Partial (preview) | Full |
| Intelligence Detail | Partial | Full |
| Country Profile | Partial | Full |
| Interactive Map | Full | Full (future layers) |
| Publications Library | Partial | Full |
| Publication Detail | Partial | Full |
| Search Results | Full | Full (more types) |
| About | Full | Full |
| Professional Access | Full | — |
| User Dashboard | — | Full |

### Content Visibility Rules

- **Public content:** always visible, never blocked
- **Professional content:** shown as locked preview with upgrade prompt
- **No empty pages:** if professional content is not available, public content fills the page
- **Preview strategy:** show at minimum 1-2 intelligence items + 1 publication on every page before the upgrade prompt

---

## 7. FEATURE MATRIX BY VERSION

### Version 1.0 (Launch)
- Home page with all sections
- 5 Country Profiles (static overview + structured sections)
- Interactive Map (MapLibre GL, hover/click/zoom/pan)
- Intelligence Feed with filters (Country, Category, Date)
- Intelligence Detail Page
- Publications Library + Publication Detail Page
- Professional Access request form + email confirmation
- Global Search with grouped results
- EN + RU language support
- Responsive design (desktop, tablet, mobile)
- Sticky header with language switcher
- Footer with all required links
- Admin Panel (editorial queue, user management)

### Version 1.5
- RSS integration + AI-assisted summarization
- Auto-categorization and tagging
- Advanced search (filters, date ranges)
- Email notifications for Professional Access users
- Improved Publications reader

### Version 2.0
- Interactive map data layers (regulatory activity, banking)
- Historical regulatory timelines
- Institution profiles
- Trade corridor pages
- AI Executive Briefs (auto-generated)

### Version 2.5
- Daily Executive Brief (personalized)
- User dashboards
- Saved searches
- Email alerts
- PDF export

### Version 3.0
- AI Assistant
- Natural language search
- Cross-country comparison
- Document comparison
- Knowledge graph
- Predictive analytics (research feature)

---

## 8. MULTILINGUAL SPECIFICATION

**Version 1 Languages:** English (primary), Russian  
**Future languages:** Without platform redesign  

**Implementation:**
- i18n library: i18next (React) or equivalent
- Translation files: JSON, per-language directory (/locales/en/, /locales/ru/)
- URL strategy: language prefix (/en/, /ru/)
- Language detection: browser navigator.language → default 'en'
- Switching: instant, no page reload, via header switcher
- Content fallback: if content exists in one language only, display with «Original language» notice

**What is translated:**
- All UI labels, navigation, buttons, forms, tooltips, notifications
- Country summaries (manually translated)
- AI-generated summaries (via AI translation API)

**What is NOT translated:**
- Original source documents (remain in source language)
- Official regulatory texts (linked externally, in source language)

---

## 9. PROFESSIONAL ACCESS MODEL

**Access Levels:**
- Level 1: Public (no registration required)
- Level 2: Professional (request + approval required)

**Request Flow:**
1. User submits form (Name, Organization, Position, Business Email, Country, Purpose)
2. System sends auto-confirmation email
3. Editorial team reviews (within 2–5 business days, v1.0)
4. Approval email sent with login credentials or activation link
5. User activates account via email link
6. User accesses Professional Dashboard

**Technical Requirements:**
- Auth: Supabase Auth (email + password)
- Session: JWT, 7-day expiry, refresh token
- RLS: Supabase Row Level Security for content visibility
- Rate limiting: 5 login attempts per 15 minutes
- Email verification: required before activation

**Professional Features (v1.0):**
- Complete Country Profiles
- Full Intelligence Archive
- Full Publications Library
- AI Executive Summaries (when available)
- PDF Downloads
- Advanced Search

---

## 10. ADMIN / EDITORIAL INTERFACE

**Location:** /admin/ (not publicly accessible)  
**Authentication:** Separate admin credentials  

**Admin Modules:**
- **Intelligence Queue:** Draft items from RSS/AI awaiting review (Approve / Edit / Reject)
- **Intelligence Manager:** Published items, edit, archive
- **Publication Manager:** Create, edit, publish, manage visibility
- **Country Data Editor:** Update Country Overview text per country
- **Source Manager:** RSS sources, add/edit/disable
- **Access Requests:** View pending Professional Access requests, approve/deny
- **User Manager:** View active Professional users, manage access

---

## 11. DEFINITION OF DONE — VERSION 1.0

Launch readiness criteria:
- [ ] All 5 Country Profiles contain baseline content
- [ ] Minimum 10 published Intelligence Items
- [ ] Minimum 3 Publications (at least 1 publicly accessible)
- [ ] Interactive Map operational (hover, click, panel, responsive)
- [ ] Professional Access request form + email confirmation working
- [ ] English and Russian localizations complete (UI strings)
- [ ] Global Search returning results across all object types
- [ ] Performance: LCP < 2.5s, FID < 100ms, CLS < 0.1
- [ ] HTTPS + security headers (HSTS, CSP, X-Frame-Options)
- [ ] Analytics (GA4 or Plausible) collecting data
- [ ] 404, 500, and Maintenance pages designed and deployed
- [ ] Mobile responsive across iPhone 12+ and standard Android
- [ ] Admin Panel operational for editorial workflow
- [ ] Privacy Policy and Terms of Use published

---

## 12. ANALYTICS SPECIFICATION

**Platform:** Google Analytics 4 (or Plausible for privacy-first option)

**Tracked Events:**

| Event Name | Trigger |
|------------|---------|
| page_view | Every page |
| search_query | Search submitted |
| search_no_results | Zero results returned |
| map_country_hover | Country hovered on map |
| map_country_click | Country clicked on map |
| intelligence_item_view | Intelligence detail opened |
| publication_view | Publication detail opened |
| publication_download | PDF download clicked |
| access_request_start | Access form opened |
| access_request_submit | Access form submitted |
| language_switch | Language changed |
| filter_applied | Intelligence/Publication filter used |

**Goals:** Professional Access submissions, Publication downloads

---

## 13. SEO SPECIFICATION

**Meta title format:** [Page Title] — CAFRI | Central Asia Financial Intelligence  
**Meta description:** Unique per page, 150–160 characters, factual  
**Open Graph:** og:title, og:description, og:image (platform logo), og:url  
**Structured Data:** Organization (home), Article (intelligence items)  
**Sitemap:** Auto-generated, submitted to Google Search Console  
**robots.txt:** Disallow /admin/, /access/dashboard/  
**Canonical URLs:** Enforced on all duplicate/paginated content  
**Language alternate:** hreflang="en" / hreflang="ru"  

---

## 14. SECURITY REQUIREMENTS

- HTTPS enforced everywhere (no HTTP fallback)
- Security headers: HSTS, CSP, X-Content-Type-Options, X-Frame-Options
- Supabase RLS for database-level content protection
- Input sanitization on all forms
- Rate limiting on: login (5/15min), search (60/min), API endpoints
- Regular automated backups
- Audit log for admin actions
- No sensitive data in client-side JavaScript

---

*This Blueprint is the authoritative product definition. All downstream documents derive from this document.*
