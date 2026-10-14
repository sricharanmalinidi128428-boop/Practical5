namespace Practical6Catalog.Models;

public sealed class CatalogViewModel
{
    public IReadOnlyList<Product> Products { get; init; } = [];
    public IReadOnlyList<string> Categories { get; init; } = [];
    public string Query { get; init; } = "";
    public string SelectedCategory { get; init; } = "All items";
    public int TotalCount { get; init; }
}
