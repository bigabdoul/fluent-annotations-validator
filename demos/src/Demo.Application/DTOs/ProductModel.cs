using FluentAnnotationsValidator.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Demo.Application.DTOs;

public class ProductModel : EntityId
{
    [Required, MaxLength(255)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public decimal Price { get; set; }

    [ExactLength(3)]
    public string? Currency { get; set; }

    [Minimum(0)]
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

    public long CatalogId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;

    public ICollection<OrderItemModel> OrderItems { get; set; } = [];
}
