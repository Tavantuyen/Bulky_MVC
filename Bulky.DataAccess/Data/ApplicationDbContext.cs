using Microsoft.EntityFrameworkCore;
using BulkyBook.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
namespace BulkyBook.DataAccess.Data
{
    public class ApplicationDbContext:IdentityDbContext<IdentityUser>
    {
   
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> option) :base(option)
        {
            
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Company> Conpanies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; } 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // phải viết  base.OnModelCreating(modelBuilder); để xác thực  IdentityDbContext
            base.OnModelCreating(modelBuilder);

      

            modelBuilder.Entity<Company>().HasData(
            new Company(){Id=1,Name="Tech Solution",PhoneNumber= "0367754060", StreetAdress = "123 Tech St", City="Tech City",State="TL",PotalCode="6699884400"},
            new Company(){Id=2,Name="Vivid Books",PhoneNumber="0984126854", StreetAdress = "999 Vivid St", City= "Vid City", State="IL",PotalCode="666654"},
            new Company(){Id=3,Name="Readers Club",PhoneNumber="0383802855", StreetAdress = "999 Main St", City="Main City",State="NV",PotalCode="94654578"}
            );

            modelBuilder.Entity<Category>().HasData(
                new Category() { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category() { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category() { Id = 3, Name = "History", DisplayOrder = 3 });
            modelBuilder.Entity<Product>().HasData(
               new Product
               {
                   Id = 1,
                   Title = "Fortune of Time",
                   Author = "Billy Spark",
                   Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                   ISBN = "SWD9999001",
                   ListPrice = 99,
                   Price = 90,
                   Price50 = 85,
                   Price100 = 80,
                   CategoryId=1,
               },
                new Product
                {
                    Id = 2,
                    Title = "Dark Skies",
                    Author = "Nancy Hoover",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "CAW777777701",
                    ListPrice = 40,
                    Price = 30,
                    Price50 = 25,
                    Price100 = 20,
                    CategoryId = 1,


                },
                new Product
                {
                    Id = 3,
                    Title = "Vanish in the Sunset",
                    Author = "Julian Button",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "RITO5555501",
                    ListPrice = 55,
                    Price = 50,
                    Price50 = 40,
                    Price100 = 35,
                    CategoryId = 1,


                },
                new Product
                {
                    Id = 4,
                    Title = "Cotton Candy",
                    Author = "Abby Muscles",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "WS3333333301",
                    ListPrice = 70,
                    Price = 65,
                    Price50 = 60,
                    Price100 = 55,
                    CategoryId = 2


                },
                new Product
                {
                    Id = 5,
                    Title = "Rock in the Ocean",
                    Author = "Ron Parker",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "SOTJ1111111101",
                    ListPrice = 30,
                    Price = 27,
                    Price50 = 25,
                    Price100 = 20,
                    CategoryId = 2,


                },
                new Product
                {
                    Id = 6,
                    Title = "Leaves and Wonders",
                    Author = "Laura Phantom",
                    Description = "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ",
                    ISBN = "FOT000000001",
                    ListPrice = 25,
                    Price = 23,
                    Price50 = 22,
                    Price100 = 20,
                    CategoryId = 3,


                }
                );
        }

    }
    
}
