using System.ComponentModel.DataAnnotations;

namespace Practical6Catalog.Models;

public sealed class Product
{
    public int Id { get; init; }

    [Required, StringLength(80)]
    public string Name { get; init; } = "";

    [Required, StringLength(40)]
    public string Category { get; init; } = "";

    [Range(0, 1_000_000)]
    public decimal Price { get; init; }

    [StringLength(240)]
    public string Description { get; init; } = "";

    public string Symbol { get; init; } = "◇";
    public string Tone { get; init; } = "sage";
    public string ImageFile { get; init; } = "";
    public string? Label { get; init; }
}
