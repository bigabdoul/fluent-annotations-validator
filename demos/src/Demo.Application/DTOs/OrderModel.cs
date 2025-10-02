using System.ComponentModel.DataAnnotations;

namespace Demo.Application.DTOs;

public class OrderModel : EntityId
{
    [MaxLength(255)]
    public string? CustomerName { get; set; }
    
    [MaxLength(20)]
    public string? CustomerPhone { get; set; }
    
    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [Required]
    public string UserId { get; set; } = default!;
    
    public ICollection<OrderItemModel> OrderItems { get; set; } = [];
}
