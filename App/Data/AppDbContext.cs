using CafeApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CafeApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderLine> OrderLines { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<WorkSession> WorkSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EntraObjectId)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .ToTable(t =>
            {
                t.IsTemporal();
                t.HasCheckConstraint("CK_Products_Price_Positive", "[Price] > 0");
            });

        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Order>()
            .HasOne(o => o.Employee)
            .WithMany()
            .HasForeignKey(o => o.EmployeeId);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(10, 2);

        modelBuilder.Entity<OrderLine>()
            .HasOne(ol => ol.Order)
            .WithMany(o => o.OrderLines)
            .HasForeignKey(ol => ol.OrderId);

        modelBuilder.Entity<OrderLine>()
            .Property(ol => ol.UnitPrice)
            .HasPrecision(10, 2);

        modelBuilder.Entity<OrderLine>()
            .Property(ol => ol.LineTotal)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Expense>()
            .Property(e => e.Amount)
            .HasPrecision(10, 2);
    }
}