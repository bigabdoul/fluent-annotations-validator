using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Demo.Infrastructure;

public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Create options builder
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        // Configure SQLite provider
        builder.UseSqlite("Data Source=app.db");

        // Pass options to AppDbContext constructor
        return new AppDbContext(builder.Options);
    }
}
