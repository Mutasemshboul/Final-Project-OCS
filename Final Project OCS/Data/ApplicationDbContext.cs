using Final_Project_OCS.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Final_Project_OCS.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSwap> ProductSwaps { get; set; }
        public DbSet<ImageItem> ImageItems { get; set; }
        public DbSet<Category>  Categories { get; set; }
        public DbSet<Subscription>   Subscriptions { get; set; }
        public DbSet<SubscriptionType>   SubscriptionTypes { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<StoreCategory> StoreCategories { get; set; }
        public DbSet<StoreProduct> StoreProducts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Item>().ToTable("Items");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductSwap>().ToTable("ProductSwaps");

            modelBuilder.Entity<ChatMessage>()
               .HasOne(cm => cm.Sender)
               .WithMany()
               .HasForeignKey(cm => cm.SenderId)
               .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete

            modelBuilder.Entity<ChatMessage>()
                .HasOne(cm => cm.Receiver)
                .WithMany()
                .HasForeignKey(cm => cm.ReceiverId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascading delete
        }
    }
}