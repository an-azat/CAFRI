# CAFRI Publication Import API

Import chain:

`Feedly -> n8n -> CAFRI API -> PostgreSQL -> Admin -> Publications`

## Endpoints

### Duplicate check

`GET /api/publications/import/check`

Query params:

- `externalId`
- `sourceUrl`

Example:

```http
GET /api/publications/import/check?externalId=feedly-article-001&sourceUrl=https://example.com/article
```

Response:

```json
{
  "isDuplicate": true,
  "matchedBy": "SourceUrl",
  "publicationId": "00000000-0000-0000-0000-000000000000",
  "workflowStatus": "Draft",
  "slug": "kazakhstan-banking-update"
}
```

### Import publication

`POST /api/publications/import`

Request body:

```json
{
  "externalId": "feedly-article-001",
  "title": "Kazakhstan banking sector update",
  "description": "Summary imported from Feedly and normalized by n8n.",
  "sourceUrl": "https://example.com/article",
  "sourceName": "Example News",
  "imageUrl": "https://example.com/image.jpg",
  "publishedAtUtc": "2026-07-24T05:00:00Z",
  "readingTimeMinutes": 7,
  "authorLabel": "Example News",
  "countries": ["Kazakhstan", "Uzbekistan"],
  "categories": ["Banking", "Analytical Briefs"],
  "requiresReview": false
}
```

Response:

```json
{
  "publicationId": "00000000-0000-0000-0000-000000000000",
  "slug": "kazakhstan-banking-sector-update",
  "workflowStatus": "Draft",
  "requiresReview": false,
  "countries": ["Kazakhstan", "Uzbekistan"],
  "categories": ["Banking", "Analytical Briefs"]
}
```

## Import rules

- CAFRI re-checks duplicates by `ExternalId` and `SourceUrl`.
- New imports are always saved with `WorkflowStatus = Draft`.
- Public website shows only records with `WorkflowStatus = Published`.
- If countries or categories are not resolved, the record is still saved as `Draft` with `RequiresReview = true`.
- `SourceUrl` is normalized before duplicate comparison.
- `Slug` is generated automatically from the title when importing.

## Admin workflow

In `Admin -> Publications`, imported records can be:

- reviewed and edited;
- filtered by country, category, source, date and status;
- moved to `Published`, `Rejected` or `Archived`.

## Authentication

If `PublicationImportApi:ApiKey` is configured, send one of:

- `X-CAFRI-Api-Key: <key>`
- `Authorization: Bearer <key>`

If the setting is empty, API key validation is skipped.
