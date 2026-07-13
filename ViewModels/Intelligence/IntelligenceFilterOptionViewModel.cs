namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceFilterOptionViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public bool IsSelected { get; init; }
}
