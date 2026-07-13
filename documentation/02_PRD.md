# CAFRI — PRODUCT REQUIREMENTS DOCUMENT (PRD)
## Version 1.0 — Implementation Ready

**Product:** CAFRI — Central Asia Financial & Regulatory Intelligence Initiative  
**Document Type:** Product Requirements Document  
**Status:** Final — Approved for Development  

---

## EXECUTIVE SUMMARY

CAFRI is a professional institutional intelligence platform focused exclusively on financial regulation, banking, trade, logistics, sanctions, and geopolitical developments across five Central Asian countries: Kazakhstan, Kyrgyzstan, Uzbekistan, Tajikistan, and Turkmenistan.

The platform serves institutional professionals — compliance officers, banking analysts, government agencies, international organizations, and corporate researchers — who require structured, reliable, and promptly-delivered regional intelligence.

Version 1.0 delivers the core platform: Home, Countries, Interactive Map, Intelligence, Publications, Search, and Professional Access, in English and Russian.

---

## 1. PRODUCT GOALS

### Primary Goal
Provide structured, reliable, and easily navigable intelligence that enables professional users to rapidly identify and assess important developments affecting Central Asia.

### Secondary Goals
- Establish CAFRI as a credible institutional intelligence source
- Demonstrate editorial discipline and analytical rigour from day one
- Create a scalable architecture capable of absorbing Version 2+ features without redesign
- Support presentation to institutional partners, investors, and regulatory audiences

### Non-Goals (Version 1.0)
- Real-time automated publishing (requires human editorial review in v1)
- AI Chat Assistant
- Personalized dashboards
- Email alert subscriptions
- Mobile application
- API access for third parties

---

## 2. FUNCTIONAL REQUIREMENTS

### FR-01: Home Page

**FR-01.1** The Home page SHALL display the following sections in this exact order: Header, Hero, Regional Pulse, Interactive Map (preview), Latest Intelligence, Featured Publication, Why CAFRI, Footer.

**FR-01.2** The Hero section SHALL occupy approximately 30% of the initial viewport height and SHALL NOT use background videos, stock photography, city imagery, or decorative graphics.

**FR-01.3** The Hero section SHALL contain: CAFRI name, positioning statement, descriptive sentence, CTA button ("Explore Intelligence"), and an animated interactive map of Central Asia on the right side.

**FR-01.4** The Regional Pulse section SHALL display five country cards, one per country, each containing: country name, count of new updates (dynamic), last updated timestamp, and "View Country" link. Cards SHALL NOT display scores, ratings, or artificial percentages.

**FR-01.5** The Latest Intelligence section SHALL display six intelligence cards with compact layout. Each card SHALL display: category, country, title (max 2 lines), summary (max 3 lines), publication date, and "Read →" link.

**FR-01.6** The Featured Publication section SHALL display one featured publication with: cover image (or typographic placeholder), title, publication date, short description, "Read Online" button, and "Download PDF" button.

**FR-01.7** The Why CAFRI section SHALL display exactly three cards: Financial Regulation, Banking Intelligence, Trade & Logistics. Each card SHALL contain one sentence explaining the value proposition. No marketing slogans.

### FR-02: Interactive Map

**FR-02.1** The Interactive Map SHALL use MapLibre GL JS as the rendering engine.

**FR-02.2** The map SHALL use GeoJSON boundaries from Natural Earth Data (1:10m scale), displaying accurate geographic boundaries for all five Central Asian countries.

**FR-02.3** Initial state: center coordinates [63.0, 43.5], zoom level 4, all five countries visible, no country selected. Instruction text: "Select a country to explore."

**FR-02.4** Hover behavior: country fill color changes smoothly (150–250ms transition), border brightens, country name label appears. No flashing or pulsing animations.

**FR-02.5** Click behavior: Country Information Panel opens on the right (desktop) or slides up from bottom (mobile). Map remains visible. Selected country stays highlighted.

**FR-02.6** Country Information Panel SHALL display: country name, overview (2–3 sentences), latest development (1–2 items), latest publication (1 item), links to Financial Regulation / Banking / Trade & Logistics sections, and "Open Country Profile" button.

**FR-02.7** The map SHALL support zoom (mouse wheel and pinch-to-zoom) and pan (drag). Min zoom: 3, Max zoom: 10.

**FR-02.8** Desktop layout: map occupies 70% of width, Country Panel occupies 30%. Divider is fixed.

**FR-02.9** Country color states: Default (#1F2937 dark gray), Hover (#2563EB blue), Selected (#2563EB blue with 2px border emphasis).

**FR-02.10** Map SHALL load within 2 seconds on standard broadband. Target 60 FPS on desktop.

**FR-02.11** Map SHALL support keyboard navigation (Tab to cycle countries, Enter to select).

**FR-02.12** Map SHALL NEVER be replaced with a static image, SVG illustration, or decorative graphic.

**FR-02.13** GeoJSON file size SHALL be optimized to ≤ 500 KB uncompressed, ≤ 150 KB gzipped.

**FR-02.14** Attribution "© OpenStreetMap contributors" SHALL be displayed (required by license).

### FR-03: Country Profiles

**FR-03.1** All five countries SHALL use an identical page structure. Only content changes.

**FR-03.2** Page structure (in order): Country Header, Country Overview, Latest Developments, Financial Regulation, Banking Sector, Trade & Logistics, Publications, Sources, Professional Intelligence.

**FR-03.3** Country Header SHALL display: country name, flag, last updated (date + UTC time), count of new intelligence items today, and action buttons (Back to Map, Share, Print, Professional Access).

**FR-03.4** Country Overview SHALL always be publicly accessible, never gated, max 250 words.

**FR-03.5** Latest Developments SHALL display 1–2 most recent intelligence items publicly. Below them: "More intelligence available with Professional Access" prompt.

**FR-03.6** Financial Regulation (public): latest regulation + short explanation + official source link. Professional: complete archive, historical timeline, cross-references, PDF download.

**FR-03.7** Banking Sector (public): latest banking development + central bank announcement + 1 publication. Professional: historical developments, institution profiles, banking timeline, AI Executive Brief.

**FR-03.8** Trade & Logistics (public): latest trade development + latest logistics update. Professional: trade corridor analysis, historical comparison, AI analysis, downloadable reports.

**FR-03.9** Publications SHALL display up to three publications (latest report, brief, analytical note). Below: "View Full Library" and Professional Access prompt.

**FR-03.10** Sources section SHALL always be publicly accessible. SHALL list official information sources with links to original publications.

**FR-03.11** Professional Intelligence section SHALL appear after all public content. SHALL list Professional features with checkmarks and "Request Professional Access" button.

**FR-03.12** Desktop: sticky sidebar SHALL show country summary, latest update, quick navigation, and Professional Access prompt while user scrolls.

**FR-03.13** In-page search SHALL allow searching within the current country only.

**FR-03.14** Breadcrumb: Home → Countries → [Country Name].

### FR-04: Intelligence Feed

**FR-04.1** Intelligence page SHALL be the primary daily workspace of the platform.

**FR-04.2** Filter panel SHALL be always visible at the top. Filters: Country (All + 5 countries), Category (8 categories), Institution (6 types), Date (date range picker), Keyword Search, Reset Filters button.

**FR-04.3** Intelligence Categories: Financial Regulation, Banking, Trade & Logistics, Sanctions, Macroeconomics, Government, Infrastructure, International Cooperation.

**FR-04.4** Each intelligence card SHALL display: category badge, country tag, date, title (max 2 lines), summary (max 3 lines), source name, "Read Full Analysis →" link.

**FR-04.5** Sorting options: Newest, Oldest, Country (A-Z), Category (A-Z), Alphabetical by title.

**FR-04.6** Pagination: load 20 items per page with "Load More" button. Infinite scroll is optional.

**FR-04.7** Mobile: filters collapse into a single dropdown/drawer. Cards occupy full width. Search remains visible.

### FR-05: Intelligence Detail Page

**FR-05.1** Intelligence Detail Page SHALL display: category badge, country tag, date, institution tag, status badge (Active / Superseded / Under Review), full title, executive summary, effective date (if applicable), official source (name + clickable URL + document type), full analysis text.

**FR-05.2** Page SHALL display: Related Regulations (up to 3), Related Intelligence Items (up to 3), Related Publications (up to 2), Institution Mini-Card, Country Mini-Card.

**FR-05.3** Share, Print, and Export buttons SHALL be available.

**FR-05.4** "Last verified" timestamp SHALL be displayed.

**FR-05.5** Breadcrumb: Home → Intelligence → [Category] → [Title].

**FR-05.6** Professional Intelligence section SHALL appear below public content.

### FR-06: Publications

**FR-06.1** Publications page SHALL function as a structured research library. It is not a blog.

**FR-06.2** Page sections: Featured Publication, Publication Categories filter, Publications Library grid, Search.

**FR-06.3** Publication Categories: Reports, Country Profiles, Analytical Briefs, Regulatory Notes, Banking Reviews, Trade & Logistics, Sanctions Analysis, Research Papers.

**FR-06.4** Each publication card SHALL display: title, category, country, publication date, reading time, author, short summary (3 lines), "Read →" and "Download PDF" buttons.

**FR-06.5** Publication Detail Page SHALL display: title, publication metadata, executive summary, main content, charts/tables (when included), references, related publications, sources, download PDF, share, print.

**FR-06.6** Filters: Country, Category, Year, Author, Keyword.

**FR-06.7** Every publication SHALL include references section with official sources, government documents, central bank publications, and academic references where applicable.

### FR-07: Search

**FR-07.1** Search SHALL index all object types: Countries, Intelligence Items, Publications, Institutions, Regulations, Sources.

**FR-07.2** Search results page SHALL group results by object type: Countries, Intelligence Items, Publications, Institutions, Regulations.

**FR-07.3** Each group SHALL display up to 5 results with "Show more" option.

**FR-07.4** Query SHALL be displayed: "Results for: [query]"

**FR-07.5** Filters: Object Type, Country, Date Range.

**FR-07.6** Sorting: Relevance (default), Newest, Oldest.

**FR-07.7** Search terms SHALL be highlighted in result excerpts.

**FR-07.8** Empty state: "No results found for '[query]'. Try a broader search or browse by country."

**FR-07.9** Search bar SHALL remain visible and accessible from every page (in header).

### FR-08: Professional Access

**FR-08.1** Professional Access page SHALL communicate capabilities, not pricing. No pricing displayed in v1.0.

**FR-08.2** Request form fields: Name (required), Organization (required), Position (required), Business Email (required), Country (required), Purpose of Access (required, textarea).

**FR-08.3** On successful submission: auto-confirmation email sent to user. Confirmation page displayed.

**FR-08.4** Confirmation page message: "Thank you. Your request has been received. Our team will contact you after review."

**FR-08.5** Admin panel SHALL show all access requests with status (Pending / Approved / Declined).

**FR-08.6** Approved users SHALL receive account activation email.

**FR-08.7** Authenticated Professional users SHALL have access to a User Dashboard at /en/access/dashboard/.

**FR-08.8** User Dashboard SHALL display: user name, organization, access status, access expiry (if applicable), and navigation to Professional content.

### FR-09: Multilingual Support

**FR-09.1** Version 1.0 SHALL support English (primary) and Russian.

**FR-09.2** Language switcher SHALL be visible in the header on every page.

**FR-09.3** Language switching SHALL NOT reload the page or reset the user's current position.

**FR-09.4** URLs SHALL use language prefix: /en/ and /ru/.

**FR-09.5** All UI elements SHALL be translated: navigation, buttons, menus, forms, tooltips, notifications, search interface, map interface.

**FR-09.6** Content not available in selected language SHALL display with a notice: "Original language: [language]."

**FR-09.7** Interface language and content language SHALL be independent.

**FR-09.8** Architecture SHALL support addition of languages without platform redesign.

### FR-10: About Page

**FR-10.1** About page SHALL contain: mission statement, methodology description, information sources policy, team information (if applicable), contact details.

**FR-10.2** Methodology section SHALL explain how intelligence is collected, processed, and published.

**FR-10.3** Sources section SHALL describe the categories of official sources used.

**FR-10.4** Contact section SHALL provide a professional contact method.

---

## 3. NON-FUNCTIONAL REQUIREMENTS

### Performance

| Metric | Target |
|--------|--------|
| Largest Contentful Paint (LCP) | < 2.5 seconds |
| First Input Delay (FID) | < 100ms |
| Cumulative Layout Shift (CLS) | < 0.1 |
| Time to Interactive (TTI) | < 3.5 seconds |
| Map initial load | < 2 seconds on broadband |
| Map frame rate (desktop) | 60 FPS target |
| Search response time | < 500ms |

### Accessibility

| Requirement | Standard |
|-------------|---------|
| Keyboard navigation | All interactive elements |
| Focus indicators | Visible, 3px ring, #2563EB |
| Color contrast | WCAG AA minimum |
| Screen reader | Semantic HTML, ARIA labels |
| Text scaling | Functional up to 200% zoom |

### Security

- HTTPS enforced (no HTTP)
- HTTP Strict Transport Security (HSTS) header
- Content Security Policy (CSP) header
- X-Frame-Options: DENY
- X-Content-Type-Options: nosniff
- Input sanitization on all form fields
- Rate limiting: login (5/15min), search (60/min)
- Supabase Row Level Security for content visibility
- No sensitive data in client-side JavaScript
- Admin panel not accessible without authentication

### Browser Support

| Browser | Minimum Version |
|---------|----------------|
| Chrome | 90+ |
| Firefox | 88+ |
| Safari | 14+ |
| Edge | 90+ |
| Mobile Safari (iOS) | 14+ |
| Chrome Android | 90+ |

### Responsive Breakpoints

| Breakpoint | Width | Layout |
|------------|-------|--------|
| Mobile | < 768px | Single column, drawer navigation |
| Tablet | 768–1024px | Two column, condensed |
| Desktop | 1024–1440px | Full layout |
| Wide Desktop | > 1440px | Max-width container, centered |

---

## 4. CONTENT REQUIREMENTS

### Minimum Content for Launch (Version 1.0)

| Content Type | Minimum Quantity |
|-------------|-----------------|
| Country Profiles (complete baseline) | 5 |
| Intelligence Items (published) | 10 |
| Publications (publicly accessible) | 3 |
| Official Sources per country | 3–5 |

### Content Standards

- No fake statistics
- No invented ratings or scores
- No AI-generated data presented as official
- All intelligence items must reference an official source
- All publications must include a references section
- AI Executive Summaries are clearly labeled as AI-generated
- Human editorial review required before publication (v1.0)

---

## 5. INTEGRATION REQUIREMENTS

### Version 1.0 (Required)
- Supabase (database + authentication + storage)
- MapLibre GL JS (interactive map)
- i18next (internationalization)
- Email delivery: Supabase email or SendGrid (for access confirmations)
- Analytics: Google Analytics 4 or Plausible

### Version 1.5 (Planned)
- RSS feed ingestion (custom service or Zapier/Make for MVP)
- AI API (OpenAI or Claude API) for summarization and categorization

### Version 2.0+ (Future)
- Advanced map data layers
- AI Executive Brief generation
- Document storage (Supabase Storage or S3)
- CDN (Cloudflare or equivalent)

---

## 6. ADMIN / EDITORIAL REQUIREMENTS

**AR-01** Admin panel SHALL be accessible at /admin/ and SHALL require separate authentication.

**AR-02** Editorial Queue SHALL display all intelligence items with status "Draft" awaiting review.

**AR-03** Editor SHALL be able to: Approve (publish immediately), Edit (modify before publishing), Reject (discard with note).

**AR-04** Intelligence Item editor SHALL provide all fields: title, country, category, institution, dates, summary, source, tags, visibility, AI summary.

**AR-05** Publication manager SHALL support: create, edit, publish, unpublish, archive, set visibility.

**AR-06** Country Data editor SHALL support editing of all country overview fields.

**AR-07** Source Manager SHALL support: add RSS source, configure country assignment, enable/disable polling.

**AR-08** Access Request manager SHALL list all requests with: name, organization, email, date, status. Actions: Approve (sends activation email), Decline (sends notification).

---

## 7. CONSTRAINTS

- Platform must not invent, generate, or display fake data at any time
- Interactive map must never be replaced with a static image
- Intelligence must never be referred to as "news"
- Publications must never be referred to as "blog posts"
- No pop-ups, countdown timers, or aggressive conversion tactics
- No stock photography anywhere on the platform
- No lorem ipsum or placeholder text in production
- Professional Access must not block all content — public value must always be delivered

---

## 8. RISKS

| Risk | Likelihood | Impact | Mitigation |
|------|-----------|--------|------------|
| Insufficient content at launch | Medium | High | Define minimum content requirements (Section 4) |
| Map performance on mobile | Medium | Medium | MapLibre GL optimization, simplified GeoJSON |
| Editorial bottleneck (v1 human review) | High | Medium | Simple queue UI, clear approve/reject workflow |
| i18n incomplete at launch | Medium | Medium | Prioritize UI strings; content follows |
| Access request volume management | Low | Low | Manual process sufficient for v1 |
| RSS sources with poor data quality | Medium | Medium | Human review gate in v1 eliminates bad data |

---

*This PRD is the authoritative requirements document for CAFRI Version 1.0 development.*
