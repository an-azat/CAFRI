namespace CAFRI.ViewModels.Intelligence;

public sealed class IntelligenceStatusRowViewModel
{
    public required string Label { get; init; }

    public required string Value { get; init; }

    public bool IsStatus { get; init; }
}
