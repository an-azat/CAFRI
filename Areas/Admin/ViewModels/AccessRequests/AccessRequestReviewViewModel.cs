using CAFRI.Domain.Access;

namespace CAFRI.Areas.Admin.ViewModels.AccessRequests;

public sealed class AccessRequestReviewViewModel
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string BusinessEmail { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PurposeOfAccess { get; set; } = string.Empty;
    public AccessRequestStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ReviewedAtUtc { get; set; }
    public string? ReviewNote { get; set; }
    public bool HasGrant { get; set; }
    public AccessGrantStatus? GrantStatus { get; set; }
    public AccessType? GrantAccessType { get; set; }
    public DateTimeOffset? GrantExpiresAtUtc { get; set; }
    public string? ActivationToken { get; set; }
    public DateTimeOffset? ActivationExpiresAtUtc { get; set; }
    public string? ActivationStatus { get; set; }
}
