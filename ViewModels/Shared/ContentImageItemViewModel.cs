namespace CAFRI.ViewModels.Shared;

public sealed class ContentImageItemViewModel
{
    public required string ImageUrl { get; set; }

    public string Alt { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Caption { get; set; } = string.Empty;
}
