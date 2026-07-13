namespace CAFRI.Infrastructure.Identity;

public sealed class AdminSeedOptions
{
    public const string SectionName = "AdminSeed";

    public string Email { get; set; } = "admin@cafri.local";
    public string Password { get; set; } = "Admin!23456";
    public string FullName { get; set; } = "CAFRI Administrator";
}
