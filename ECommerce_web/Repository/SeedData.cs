using ECommerce_web.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Repository
{
    public class SeedData
    {
        public static void SeedingData(DataContext _context)
        {
            _context.Database.Migrate();
            if (!_context.Products.Any())
            {
                CategoryModel macbook = new CategoryModel { Name = "Macbook", Slug = "macbook", Description = "Macbook is Large Product in the World", Status = 1 };
                CategoryModel pc = new CategoryModel { Name = "Pc", Slug = "pc", Description = "PC is Large Product in the World", Status = 1 };

                BrandModel apple = new BrandModel { Name = "Apple", Slug = "apple", Description = "Apple is Large Product in the World", Status = 1 };
                BrandModel samsung = new BrandModel { Name = "SamSung", Slug = "samsung", Description = "SamSung is Large Product in the World", Status = 1 };

                _context.Products.AddRange(
                    new ProductModel { Name = " Macbook", Slug = "macbook", Description = "Macbook is the best", Image = "1.jpg", Category = macbook, Brand = apple, Price = 122},
                    new ProductModel { Name = "Pc", Slug = "PC", Description = "PC is the best", Image = "1.jpg", Category = pc, Brand = samsung, Price = 1224}
                );

                _context.SaveChanges();
            }
        }
    }
}
