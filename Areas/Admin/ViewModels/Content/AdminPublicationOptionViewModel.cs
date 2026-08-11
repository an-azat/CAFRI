namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class AdminPublicationOptionViewModel
{
    public Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Meta { get; init; }
}
