using Demo.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Demo.Infrastructure.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public ICollection<Catalog> Catalogs { get; set; } = [];
    public ICollection<Product> Products { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
}
