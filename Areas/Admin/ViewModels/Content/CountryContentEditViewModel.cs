using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CAFRI.Areas.Admin.ViewModels.Content;

public sealed class CountryContentEditViewModel
{
    public Guid? Id { get; init; }

    [Required, StringLength(10)]
    public string Code { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string Slug { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string Capital { get; init; } = string.Empty;

    [Required, StringLength(1000)]
    public string Summary { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Gdp { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string Population { get; init; } = string.Empty;

    [Required, StringLength(100)]
    public string BankAssets { get; init; } = string.Empty;

    [Required, StringLength(150)]
    public string HeroClassName { get; init; } = string.Empty;

    public string? HeroGalleryText { get; init; }

    public IFormFile[]? HeroGalleryFiles { get; init; }

    [Required]
    public string DetailsJson { get; init; } = string.Empty;

    public int DisplayOrder { get; init; }

    public bool IsPublished { get; init; } = true;
}
