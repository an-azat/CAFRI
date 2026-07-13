using CAFRI.Domain.Users;

namespace CAFRI.Domain.Access;

public sealed class AccessRequest
{
    public Guid Id { get; set; }
    public string? ApplicationUserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string BusinessEmail { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PurposeOfAccess { get; set; } = string.Empty;

    public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;
    public string? ReviewedByUserId { get; set; }
    public DateTimeOffset? ReviewedAtUtc { get; set; }
    public string? ReviewNote { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public ApplicationUser? ApplicationUser { get; set; }
    public ICollection<ProfessionalAccessGrant> Grants { get; set; } = new List<ProfessionalAccessGrant>();
}
