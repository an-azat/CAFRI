namespace CAFRI.Domain.Content;

/// <summary>
/// Who can see a published publication on the public site.
/// </summary>
public static class PublicationVisibility
{
    // Full text is readable by everyone, including anonymous visitors.
    public const string Public = "Public";

    // Listed for everyone, but the full text requires signing in.
    public const string SignedIn = "SignedIn";

    // Not listed and not reachable on the public site.
    public const string Hidden = "Hidden";

    public static readonly IReadOnlyList<string> All = [Public, SignedIn, Hidden];

    public static string Normalize(string? value) =>
        All.FirstOrDefault(item => string.Equals(item, value?.Trim(), StringComparison.OrdinalIgnoreCase)) ?? SignedIn;

    public static string GetLabel(string? value) => Normalize(value) switch
    {
        Public => "Everyone",
        Hidden => "Hidden",
        _ => "Signed-in users only"
    };

    public static string GetAccessLabel(string? value) =>
        Normalize(value) == Public ? "Public" : "Sign-in required";
}
