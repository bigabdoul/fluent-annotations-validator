namespace Demo.Domain.Entities;

public class OrderItem
{
    public long Id { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public long ProductId { get; set; }
    public Product Product { get; set; } = default!;
    public long OrderId { get; set; }
    public Order Order { get; set; } = default!;
}
