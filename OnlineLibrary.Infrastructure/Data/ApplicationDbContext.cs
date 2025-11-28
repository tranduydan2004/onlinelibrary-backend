using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Domain.Entities;

namespace OnlineLibrary.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        // Constructor nhận DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Khai báo các DbSet, tương ứng với các bảng trong CSDL
        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<BookInventory> BookInventories { get; set; }
        public DbSet<LoanRequest> LoanRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ 1-1 giữa Book và BookInventory
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Inventory)
                .WithOne(i => i.Book)
                .HasForeignKey<BookInventory>(i => i.BookId)
                .IsRequired(); // Đảm bảo mỗi sách phải có một bản ghi kho

            // Cấu hình mối quan hệ 1-N giữa User và LoanRequest
            modelBuilder.Entity<User>()
                .HasMany(u => u.LoanRequests)
                .WithOne(l => l.User)
                .HasForeignKey(l => l.UserId)
                .IsRequired();

            // Cấu hình mối quan hệ 1-N giữa Book và LoanRequest
            modelBuilder.Entity<Book>()
                .HasMany(b => b.LoanRequests)
                .WithOne(l => l.Book)
                .HasForeignKey(l => l.BookId)
                .IsRequired();

            // Đảm bảo tên người dùng là duy nhất
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
        }
    }
}
