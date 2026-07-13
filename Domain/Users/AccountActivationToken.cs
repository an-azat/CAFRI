namespace CAFRI.Domain.Users;

public sealed class AccountActivationToken
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public Guid? AccessRequestId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }

    public ApplicationUser User { get; set; } = null!;
}
