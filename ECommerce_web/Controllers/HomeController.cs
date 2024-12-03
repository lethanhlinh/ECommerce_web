using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ECommerce_web.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager <AppUserModel> _userManager;
        public HomeController(ILogger<HomeController> logger, DataContext context, UserManager<AppUserModel> userManager)
        {
            _logger = logger;
            _dataContext = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var products = _dataContext.Products.Include("Category").Include("Brand").ToList();
            var sliders = _dataContext.Sliders.Where(s => s.Status ==1).ToList();
            ViewBag.Sliders = sliders;

            return View(products);
        }

		//Hàm compare
        public async Task <IActionResult> Compare()
        {
            var compare_product = await (from c in _dataContext.Compares
                                          join p in _dataContext.Products on c.ProductId equals p.Id
                                          join u in _dataContext.Users on c.UserId equals u.Id
                                          select new { User = u, Product = p, Compares = c }).ToListAsync();
            return View(compare_product);
        }

        //Xóa Compare
        public async Task<IActionResult> DeleteCompare(int Id)
        {
            CompareModel compare = await _dataContext.Compares.FindAsync(Id);


            _dataContext.Compares.Remove(compare);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã được xóa khỏi mục so sánh!!!";
            return RedirectToAction("Compare", "Home");
        }
        //Hàm wishlist
        public async Task<IActionResult> Wishlist()
        {
            var wishlist_product = await (from w in _dataContext.Wishlists
                                          join p in _dataContext.Products on w.ProductId equals p.Id
                                          join u in _dataContext.Users on w.UserId equals u.Id 
                                          select new { User = u, Product = p, Wishlist = p }).ToListAsync();
            return View(wishlist_product);
        }

        //Hàm xóa wishlist
        public async Task<IActionResult> DeleteWishlist(int Id)
        {
            WishlistModel wishlist = await _dataContext.Wishlists.FindAsync(Id);


            _dataContext.Wishlists.Remove(wishlist);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã được xóa khỏi mục yêu thích!!!";
            return RedirectToAction("Wishlist", "Home");
        }


        public async Task< IActionResult> AddWishlist(long Id, WishlistModel wishlistmodel)
		{
			var user =await _userManager.GetUserAsync(User);

            var wishlistProduct = new WishlistModel
            {
                ProductId = Id,
                UserId = user.Id,
            };
            
            _dataContext.Wishlists.Add(wishlistProduct);
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = " Add to wishlist Successfuly" });
            }
            catch (Exception ex)
            {
                   return StatusCode(500, "An error accurred while updating the order status.");
            }

		}

		public async Task<IActionResult> AddCompare(long Id)
		{
			var user = await _userManager.GetUserAsync(User);

			var compareProduct = new CompareModel
			{
				ProductId = Id,
				UserId = user.Id,
			};

			_dataContext.Compares.Add(compareProduct);
			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = " Add to compare Successfuly" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, "An error accurred while updating the compare table.");
			}

		}



		public IActionResult Privacy()
        {
            return View();
        }
        public async Task<IActionResult> Contact()
        {
            var contact = await _dataContext.Contact.FirstAsync();
            return View(contact);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int statuscode)
        {
            if (statuscode == 404)
            {
                return View("NotFound");
            }
            else
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }
    }
}
