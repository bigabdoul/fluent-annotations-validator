using System.ComponentModel.DataAnnotations;

namespace Demo.Domain.Entities;

public class Catalog
{
    public long Id { get; set; }

    [MaxLength(255)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool Active { get; set; }

    public string UserId { get; set; } = default!;
        
    public ICollection<Product> Products { get; set; } = [];
}
