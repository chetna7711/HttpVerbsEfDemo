using HttpVerbsEfDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace HttpVerbsEfDemo.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Category).HasMaxLength(60).IsRequired();
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CreatedAtUtc).HasDefaultValueSql("GETUTCDATE()");
        });
    }
}
