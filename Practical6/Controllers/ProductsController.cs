using Microsoft.AspNetCore.Mvc;
using Practical6Catalog.Models;

namespace Practical6Catalog.Controllers;

public sealed class ProductsController : Controller
{
    private static readonly IReadOnlyList<Product> Catalog =
    [
        new() { Id = 101, Name = "Studio Wireless Headphones", Category = "Audio", Price = 3490m, Description = "Comfortable over-ear listening with a clear, balanced sound for study sessions and everyday travel.", Symbol = "◉", Tone = "plum", ImageFile = "headphones.png", Label = "Bestseller" },
        new() { Id = 102, Name = "Arc Desk Lamp", Category = "Workspace", Price = 1890m, Description = "A softly diffused, adjustable lamp that brings focused light to your desk without visual clutter.", Symbol = "♧", Tone = "gold", ImageFile = "desk-lamp.png", Label = "New" },
        new() { Id = 103, Name = "Field Notes Backpack", Category = "Bags", Price = 2790m, Description = "A lightweight daily backpack with a padded laptop sleeve and pockets for campus essentials.", Symbol = "▱", Tone = "blue", ImageFile = "backpack.png", Label = null },
        new() { Id = 104, Name = "Everyday Steel Bottle", Category = "Lifestyle", Price = 790m, Description = "A durable, reusable bottle with a leak-resistant lid for long days between classes.", Symbol = "◒", Tone = "mint", ImageFile = "bottle.png", Label = "Popular" },
        new() { Id = 105, Name = "Mechanical Keyboard Mini", Category = "Workspace", Price = 4290m, Description = "A compact mechanical keyboard with tactile keys and a clean layout for productive work.", Symbol = "▦", Tone = "rose", ImageFile = "keyboard.png", Label = null },
        new() { Id = 106, Name = "Pocket Bluetooth Speaker", Category = "Audio", Price = 2190m, Description = "Room-filling sound in a small, carry-anywhere speaker made for shared study breaks.", Symbol = "♫", Tone = "orange", ImageFile = "speaker.png", Label = null },
        new() { Id = 107, Name = "Canvas Utility Tote", Category = "Bags", Price = 990m, Description = "A sturdy cotton tote with a roomy interior for books, notebooks, and daily carry.", Symbol = "▤", Tone = "sage", ImageFile = "tote.png", Label = null },
        new() { Id = 108, Name = "Ceramic Desk Planter", Category = "Lifestyle", Price = 650m, Description = "A small glazed ceramic planter to bring a little greenery to a desk or bedside table.", Symbol = "♧", Tone = "green", ImageFile = "planter.png", Label = "Small batch" }
    ];

    [HttpGet]
    public IActionResult Index(string? q, string? category)
    {
        q = q?.Trim() ?? "";
        category = string.IsNullOrWhiteSpace(category) ? "All items" : category.Trim();

        IEnumerable<Product> results = Catalog;
        if (q.Length > 0)
            results = results.Where(product => product.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                || product.Category.Contains(q, StringComparison.OrdinalIgnoreCase)
                || product.Description.Contains(q, StringComparison.OrdinalIgnoreCase));
        if (category != "All items")
            results = results.Where(product => product.Category.Equals(category, StringComparison.OrdinalIgnoreCase));

        var model = new CatalogViewModel
        {
            Products = results.ToArray(),
            Categories = Catalog.Select(product => product.Category).Distinct().Order(StringComparer.Ordinal).ToArray(),
            Query = q,
            SelectedCategory = category,
            TotalCount = Catalog.Count
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Details(int id)
    {
        var product = Catalog.FirstOrDefault(item => item.Id == id);
        return product is null ? NotFound() : View(product);
    }
}
