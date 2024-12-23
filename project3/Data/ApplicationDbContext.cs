namespace Project3.Data
{
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
        public DbSet<Feedbacks> Feedbacks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình CustomerInformationId với độ dài cố định 8 ký tự
            modelBuilder.Entity<CustomerInformation>()
                .Property(ci => ci.CustomerInformationId)
                .HasMaxLength(8)
                .IsFixedLength();

            // Cấu hình chuyển đổi enum trạng thái OrderDetail sang chuỗi
            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Status)
                .HasConversion<string>();

            // Cấu hình quan hệ giữa CustomerInformation và Class
            modelBuilder.Entity<CustomerInformation>()
                .HasOne(ci => ci.Class)
                .WithMany(c => c.CustomerInformations)
                .HasForeignKey(ci => ci.ClassesId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa Class nếu CustomerInformation còn tồn tại

            // Cấu hình quan hệ giữa CustomerInformation và Course
            modelBuilder.Entity<CustomerInformation>()
                .HasOne(ci => ci.Course)
                .WithMany(c => c.CustomerInformations)
                .HasForeignKey(ci => ci.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa CustomerInformation khi Course bị xóa

            // Cấu hình quan hệ giữa Course và Class
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Classes)
                .WithOne(cls => cls.Course)
                .HasForeignKey(cls => cls.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa Class khi Course bị xóa

            // Cấu hình quan hệ giữa Course và OrderDetail
            modelBuilder.Entity<Course>()
                .HasMany(c => c.OrderDetails)
                .WithOne(od => od.Course)
                .HasForeignKey(od => od.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // Xóa OrderDetail khi Course bị xóa

            // Cấu hình quan hệ giữa Course và Category
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Không xóa Category khi Course bị xóa

            base.OnModelCreating(modelBuilder);
        }
    }
}
