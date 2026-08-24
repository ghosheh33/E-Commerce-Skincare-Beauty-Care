using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce_Skincare_Beauty_Care.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Testimonial> Testimonials { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // هذا السطر ضروري جداً لإنشاء جداول Identity الأساسية
            base.OnModelCreating(builder);

            // 1. تغيير أسماء جداول Identity لتكون أنظف في SQL (خطوة اختيارية ولكنها احترافية)
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");

            // 2. حل مشكلة (Cascade Delete) لمنع أخطاء SQL Server

            // منع حذف تفاصيل الطلب (OrderItem) إذا تم حذف المنتج (Product)
            // لحماية الفواتير والطلبات السابقة من الضياع
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // منع حذف الطلب (Order) إذا تم حذف المستخدم (User) لحفظ السجلات المالية
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ربط قائمة الأمنيات بالمنتج (إذا انحذف المنتج، تنحذف الأمنية)
            builder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.WishlistedBy)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // ربط التقييمات بالمنتج (إذا انحذف المنتج، تنحذف تقييماته)
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }




}