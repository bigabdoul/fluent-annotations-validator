using FluentAnnotationsValidator.Annotations;

namespace Demo.Application.DTOs;

public class OrderItemModel : EntityId
{
    public long OrderId { get; set; }
    public long ProductId { get; set; }

    [Minimum(1)]
    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
