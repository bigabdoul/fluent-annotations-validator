using System.ComponentModel.DataAnnotations;

namespace Demo.Domain.Entities;

public class Product
{
    public long Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = default!;
    public long CatalogId { get; set; }
    public Catalog Catalog { get; set; } = default!;
    public string UserId { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Currency { get; set; }

    public int Stock { get; set; }

    [MaxLength(50)]
    public string? SKU { get; set; }

    [MaxLength(50)]
    public string? Unit { get; set; }

    public bool IsVisible { get; set; }

    [MaxLength(255)]
    public string? ImageUrl { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
