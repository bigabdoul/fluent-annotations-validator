using System.ComponentModel.DataAnnotations;

namespace Demo.Application.DTOs;

public class CatalogModel : EntityId
{
    [Required, MaxLength(255)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool Active { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
}
