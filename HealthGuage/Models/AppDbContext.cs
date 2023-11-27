using HealthGuage.HelpingClasses;
using Template.HelpingClasses;
using Microsoft.EntityFrameworkCore;
using Template.Models;

namespace HealthGuage.Models
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options): base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Name = "Uzair Aslam",
                    PhoneNumber = "0000-0000000",
                    Email = "uzair.aslam02@gmail.com",
                    Password = StringCipher.EncryptString("123"),
                    Role = 1,
                    ProfilePicture = null,
                    IsActive = 1,
                    CreatedAt = GeneralPurpose.DateTimeNow()
                }
            );
        }

        public DbSet<User> User { get; set; }
        public DbSet<ContentFile> ContentFile { get; set; }
        public DbSet<Ingredient> Ingredient { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<Preperation> Preperation { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<MenuIngredient> MenuIngredient { get; set; }
        public DbSet<MenuPreperation> MenuPreperation { get; set; }
        public DbSet<MenuProduct> MenuProduct { get; set; }
    }
}
