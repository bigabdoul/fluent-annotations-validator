using System.ComponentModel.DataAnnotations;

namespace Demo.Domain.Entities;

public class Order
{
    public long Id { get; set; }
    [MaxLength(255)]
    public string CustomerName { get; set; } = default!;

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";
    public string UserId { get; set; } = default!;
    public ICollection<OrderItem> OrderItems { get; set; } = [];
}
