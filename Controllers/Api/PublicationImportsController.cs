using CAFRI.Domain.Content;
using CAFRI.Infrastructure.Content;
using CAFRI.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CAFRI.Controllers.Api;

[ApiController]
[AllowAnonymous]
[Route("api/publications/import")]
public sealed class PublicationImportsController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly PublicationImportApiOptions _options;

    public PublicationImportsController(
        AppDbContext dbContext,
        IOptions<PublicationImportApiOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    [HttpGet("check")]
    public async Task<ActionResult<PublicationImportDuplicateCheckResponse>> CheckDuplicate(
        [FromQuery] PublicationImportDuplicateCheckRequest request,
        CancellationToken cancellationToken)
    {
        if (!AuthorizeRequest())
        {
            return Unauthorized();
        }

        var duplicate = await FindDuplicateAsync(request.ExternalId, request.SourceUrl, cancellationToken);
        if (duplicate is null)
        {
            return Ok(new PublicationImportDuplicateCheckResponse { IsDuplicate = false });
        }

        return Ok(new PublicationImportDuplicateCheckResponse
        {
            IsDuplicate = true,
            MatchedBy = ResolveMatchedBy(duplicate, request.ExternalId, request.SourceUrl),
            PublicationId = duplicate.Id,
            WorkflowStatus = duplicate.WorkflowStatus,
            Slug = duplicate.Slug
        });
    }

    [HttpPost]
    public async Task<ActionResult<PublicationImportResponse>> Import(
        [FromBody] PublicationImportRequest request,
        CancellationToken cancellationToken)
    {
        if (!AuthorizeRequest())
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var duplicate = await FindDuplicateAsync(request.ExternalId, request.SourceUrl, cancellationToken);
        if (duplicate is not null)
        {
            return Conflict(new PublicationImportDuplicateCheckResponse
            {
                IsDuplicate = true,
                MatchedBy = ResolveMatchedBy(duplicate, request.ExternalId, request.SourceUrl),
                PublicationId = duplicate.Id,
                WorkflowStatus = duplicate.WorkflowStatus,
                Slug = duplicate.Slug
            });
        }

        var publicationId = Guid.NewGuid();
        var countryAssignments = await PublicationImportHelpers.ResolveCountryAssignmentsAsync(
            _dbContext,
            request.Countries ?? [],
            publicationId,
            cancellationToken);
        var categoryAssignments = await PublicationImportHelpers.ResolveCategoryAssignmentsAsync(
            _dbContext,
            request.Categories ?? [],
            publicationId,
            cancellationToken);

        var requiresReview = request.RequiresReview || countryAssignments.Count == 0 || categoryAssignments.Count == 0;
        var title = request.Title.Trim();
        var description = request.Description.Trim();
        var sourceUrl = PublicationImportHelpers.NormalizeSourceUrl(request.SourceUrl);
        var sourceDomain = PublicationImportHelpers.ExtractDomain(request.SourceUrl);
        var sourceName = string.IsNullOrWhiteSpace(request.SourceName) ? sourceDomain : request.SourceName.Trim();

        var primaryCountry = countryAssignments.FirstOrDefault();
        var primaryCategory = categoryAssignments.FirstOrDefault();

        var slugBase = PublicationImportHelpers.GenerateSlug(title);
        var slug = await EnsureUniqueSlugAsync(slugBase, cancellationToken);

        var publicationDate = request.PublishedAtUtc?.UtcDateTime.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture)
            ?? DateTime.UtcNow.ToString("MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
        var readingTime = request.ReadingTimeMinutes.HasValue && request.ReadingTimeMinutes.Value > 0
            ? $"{request.ReadingTimeMinutes.Value} min read"
            : "8 min read";

        var entity = new PublicationContentItem
        {
            Id = publicationId,
            Slug = slug,
            WorkflowStatus = PublicationWorkflowStatuses.Draft,
            RequiresReview = requiresReview,
            ExternalId = string.IsNullOrWhiteSpace(request.ExternalId) ? null : request.ExternalId.Trim(),
            SourceName = sourceName,
            SourceUrl = sourceUrl,
            SourceDomain = sourceDomain,
            Type = "REPORT",
            CountryCode = primaryCountry?.CountryCode ?? "RG",
            CountryLabel = primaryCountry?.CountryLabel ?? "Regional",
            Title = title,
            Description = description,
            Meta = $"{publicationDate} | {readingTime}",
            Topic = primaryCategory?.CategoryName ?? "Uncategorized",
            PublishedDate = publicationDate,
            ReadingTime = readingTime,
            AuthorLabel = string.IsNullOrWhiteSpace(request.AuthorLabel) ? sourceName : request.AuthorLabel.Trim(),
            DocumentLabel = "External source article",
            HeroVisualClassName = "publication-hero-media--trade-corridors",
            HeroImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
            PdfDownloadUrl = request.SourceUrl.Trim(),
            DisplayOrder = 9999,
            IsPublished = false,
            SourcePublishedAtUtc = request.PublishedAtUtc,
            ImportedAtUtc = DateTimeOffset.UtcNow,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            CountryAssignments = countryAssignments,
            CategoryAssignments = categoryAssignments
        };

        _dbContext.PublicationContentItems.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new PublicationImportResponse
        {
            PublicationId = entity.Id,
            Slug = entity.Slug,
            WorkflowStatus = entity.WorkflowStatus,
            RequiresReview = entity.RequiresReview,
            Countries = entity.CountryAssignments.Select(x => x.CountryLabel).ToList(),
            Categories = entity.CategoryAssignments.Select(x => x.CategoryName).ToList()
        });
    }

    private bool AuthorizeRequest()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            return true;
        }

        var provided = Request.Headers["X-CAFRI-Api-Key"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(provided))
        {
            var authorization = Request.Headers.Authorization.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(authorization) &&
                authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                provided = authorization["Bearer ".Length..].Trim();
            }
        }

        return string.Equals(provided, _options.ApiKey, StringComparison.Ordinal);
    }

    private async Task<PublicationContentItem?> FindDuplicateAsync(
        string? externalId,
        string? sourceUrl,
        CancellationToken cancellationToken)
    {
        var normalizedExternalId = string.IsNullOrWhiteSpace(externalId) ? null : externalId.Trim();
        var normalizedSourceUrl = PublicationImportHelpers.NormalizeSourceUrl(sourceUrl);

        if (string.IsNullOrWhiteSpace(normalizedExternalId) && string.IsNullOrWhiteSpace(normalizedSourceUrl))
        {
            return null;
        }

        return await _dbContext.PublicationContentItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                (!string.IsNullOrWhiteSpace(normalizedExternalId) && x.ExternalId == normalizedExternalId) ||
                (!string.IsNullOrWhiteSpace(normalizedSourceUrl) && x.SourceUrl == normalizedSourceUrl),
                cancellationToken);
    }

    private static string ResolveMatchedBy(PublicationContentItem duplicate, string? externalId, string? sourceUrl)
    {
        if (!string.IsNullOrWhiteSpace(externalId) &&
            string.Equals(duplicate.ExternalId, externalId.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return "ExternalId";
        }

        if (!string.IsNullOrWhiteSpace(sourceUrl) &&
            string.Equals(duplicate.SourceUrl, PublicationImportHelpers.NormalizeSourceUrl(sourceUrl), StringComparison.OrdinalIgnoreCase))
        {
            return "SourceUrl";
        }

        return "Unknown";
    }

    private async Task<string> EnsureUniqueSlugAsync(string slugBase, CancellationToken cancellationToken)
    {
        var candidate = slugBase;
        var suffix = 2;

        while (await _dbContext.PublicationContentItems.AsNoTracking().AnyAsync(x => x.Slug == candidate, cancellationToken))
        {
            candidate = $"{slugBase}-{suffix++}";
        }

        return candidate;
    }
}
