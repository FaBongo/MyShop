using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyShop.Auth;
using MyShop.Models.Domains;
using System.Reflection.Emit;

namespace MyShop.Data
{
    public class DbMyShopContext : DbContext//IdentityDbContext<User>
    {
        public DbMyShopContext(DbContextOptions<DbMyShopContext> options)
            : base(options)
        {

        }
        /// <summary>
        /// Permet de faire un link pour permettre la création de notre base de données
        /// </summary>
        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Bill> Bills { get; set; }

        public DbSet<Payment> Payment { get; set; }

        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuration des clés étrangères

            builder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(pc => pc.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Product>()
           .HasOne(a => a.Category)
           .WithMany(c => c.Products)
           .HasForeignKey(a => a.IdCategorie);

            // Configuration de la date d'enregistrement
            builder.Entity<Product>()
                .Property(a => a.RegistrationDate)
                .HasDefaultValueSql("GETDATE()"); // Utilise la fonction SQL GETDATE() pour définir la valeur par défaut

        builder.Entity<Account>()
            .HasOne(a => a.User)
            .WithMany(c => c.Accounts)
            .HasForeignKey(a => a.IdUser);

            builder.Entity<Bill>()
            .HasOne(a => a.User)
            .WithMany(c => c.Bills)
            .HasForeignKey(a => a.IdUser)
            .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Bill>()
            .HasOne(a => a.Order)
            .WithMany(c => c.Bills)
            .HasForeignKey(a => a.IdOrder);

            builder.Entity<Payment>()
            .HasOne(a => a.User)
            .WithMany(c => c.Payments)
            .HasForeignKey(a => a.IdUser)
            .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Payment>()
            .HasOne(a => a.Order)
            .WithMany(c => c.Payments)
            .HasForeignKey(a => a.IdOrder);

            builder.Entity<Order>()
            .HasOne(a => a.User)
            .WithMany(c => c.Orders)
            .HasForeignKey(a => a.IdUser)
            .OnDelete(DeleteBehavior.ClientSetNull);

            builder.Entity<Order>()
            .HasOne(a => a.Product)
            .WithMany(c => c.Orders)
            .HasForeignKey(a => a.IdProduct);
        }


       
    }
}
