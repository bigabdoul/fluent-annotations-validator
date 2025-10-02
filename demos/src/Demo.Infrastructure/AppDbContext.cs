using Demo.Domain.Entities;
using Demo.Infrastructure.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Demo.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Catalog> Catalogs => Set<Catalog>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ApplicationUser>()
            .HasKey(u => u.Id);

        modelBuilder.Entity<Catalog>()
            .HasKey(c => c.Id);
        modelBuilder.Entity<Catalog>()
            .Property(c => c.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Catalog>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Catalogs)
            .HasForeignKey(c => c.UserId);

        modelBuilder.Entity<Product>()
            .HasKey(p => p.Id);
        modelBuilder.Entity<Product>()
            .Property(p => p.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Catalog)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CatalogId);
        modelBuilder.Entity<Product>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Products)
            .HasForeignKey(p => p.UserId);

        modelBuilder.Entity<Order>()
            .HasKey(o => o.Id);
        modelBuilder.Entity<Order>()
            .Property(o => o.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<Order>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<OrderItem>()
            .HasKey(oi => oi.Id);
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.Id)
            .ValueGeneratedOnAdd();
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Product)
            .WithMany(p => p.OrderItems)
            .HasForeignKey(oi => oi.ProductId);
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId);
    }
}
