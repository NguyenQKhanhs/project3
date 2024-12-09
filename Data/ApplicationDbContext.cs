namespace Project3.Data;
using Microsoft.EntityFrameworkCore;
using Project3.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Course> Courses { get; set; }
    public DbSet<CustomerInformation> CustomerInformations { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Discount> Discounts { get; set; }
    public DbSet<Token> Tokens { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Instructor> Instructors { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CustomerInformationId: Fixed Length
        modelBuilder.Entity<CustomerInformation>()
            .Property(ci => ci.CustomerInformationId)
            .HasMaxLength(8)
            .IsFixedLength();

        // Enum for OrderDetail Status
        modelBuilder.Entity<OrderDetail>()
            .Property(od => od.Status)
            .HasConversion<string>();

        base.OnModelCreating(modelBuilder);
    }
}