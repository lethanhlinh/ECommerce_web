using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace ECommerce_web.Controllers
{
	public class CartController : Controller
	{
		private readonly DataContext _dataContext;
		public CartController(DataContext _context)
		{
			_dataContext = _context;
		}
		public IActionResult Index()
		{
			List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			//Nhận shippingPrice từ cookie
			var shippingPriceCookie = Request.Cookies["ShippingPrice"];
            decimal shippingPrice = 0;

            //Nhận couponCode từ cookie 
            var coupon_code = Request.Cookies["CouponTitle"];

			if (shippingPriceCookie != null) {
				var shippingPriceJson = shippingPriceCookie;
				shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);	
			}

			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price),
				ShippingCost = shippingPrice,
				CouponCode = coupon_code
			};
			return View(cartVM);
		}
		public IActionResult Checkout()
		{
			return View("~/Views/Checkout/Index.cshtml");
		}
		public async Task<IActionResult> Add(long Id)
		{
			ProductModel product = await _dataContext.Products.FindAsync(Id);

			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();

			CartItemModel cartItems = cart.Where(c => c.ProductId == Id).FirstOrDefault();

			if (cartItems == null)
			{
				cart.Add(new CartItemModel(product));
			}
			else
			{
				cartItems.Quantity += 1;
			}

			HttpContext.Session.SetJson("Cart", cart);

			//Hien thi thong bao
			TempData["success"] = "Add Item to cart Successfully! " ;

			return Redirect(Request.Headers["Referer"].ToString());
		}

		//Ham giam
		public async Task<IActionResult> Decrease(long Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			// lay san pham bang id cua san pham can giam so luong

			if (cartItem.Quantity > 1)
			{
				--cartItem.Quantity;
				// giam so luong 
			}
			else
			{
				cart.RemoveAll(p => p.ProductId == Id);
				// xoa luon san pham do
			}
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");

			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
				//tao session gio hang moi
			}
            TempData["success"] = "Decrease Item quantity to cart Successfully! ";
            return RedirectToAction("Index");
		}

		//Ham tang 
		public async Task<IActionResult> Increase(long Id)
		{
			ProductModel product = await _dataContext.Products.Where(p => p.Id == Id).FirstOrDefaultAsync();
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			// lay san pham bang id cua san pham can giam so luong

			if (cartItem.Quantity >= 1 && product.Quantity > cartItem.Quantity )
			{
				++cartItem.Quantity;
                TempData["success"] = "Increase Product to cart Sucessfully! ";
                // tang so luong 
            }
			else
			{
				cartItem.Quantity = product.Quantity;
				TempData["success"] = "Maxinum Product Quantity to cart Sucessfully! ";
			//	// xoa luon san pham do
			}
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");

			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
				//tao session gio hang moi
			}
            TempData["success"] = "Increase Item quantity to cart Successfully! ";
            return RedirectToAction("Index");
		}
		public async Task<IActionResult> Remove(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");
			cart.RemoveAll(p => p.ProductId == Id);
			//xoa gio hang theo id
			if (cart.Count == 0)
			{
				HttpContext.Session.Remove("Cart");
				//neu xoa den san pham cuoi cung thi xoa session gio hang
			}
			else
			{
				HttpContext.Session.SetJson("Cart", cart);
				//set lai session(so luong)
			}
            TempData["success"] = "Remove Item of cart Successfully! ";
            return RedirectToAction("Index");
		}
		public async Task<IActionResult> Clear(long id)
		{
			//xoa het gio hang
			HttpContext.Session.Remove("Cart");
            TempData["success"] = "Clear all Item of cart Successfully! ";
            return RedirectToAction("Index");
		}
		[HttpPost]
		[Route("Cart/GetShipping")]
		public async Task<IActionResult> GetShipping(ShippingModel shipping, string tinh, string quan, string phuong)
		{
            var existingShipping = await _dataContext.Shippings
                    .FirstOrDefaultAsync(x => x.City == tinh && x.District == quan && x.Ward == phuong);
			
			decimal shippingPrice = 0;//Set mặc định giá tiền

			if(existingShipping != null)
			{
				shippingPrice = existingShipping.Price;
			}
			else
			{
				//Set mặc định tiền nếu kh tìm thấy
				shippingPrice = 50000;
			}

			var shippingPriceJson = JsonConvert.SerializeObject(shippingPrice);

			try
			{
				var cookieOptions = new CookieOptions
				{
					HttpOnly = true,
					Expires = DateTimeOffset.UtcNow.AddMinutes(30),
					Secure = true //using Https
				};

				Response.Cookies.Append("ShippingPrice", shippingPriceJson, cookieOptions);
			}
			catch (Exception ex) { 
				Console.WriteLine($"Error adding shipping price cookie: {ex.Message}");
			}
			return Json(new {shippingPrice});
        }

		[HttpGet]
		[Route("Cart/DeleteShipping")]
		public IActionResult DeleteShipping()
		{
			Response.Cookies.Delete("ShippingPrice");
			//return Json(new {success = true});
			return RedirectToAction("Index","Cart");
		}

        //Hàm GetCoupon
        [HttpPost]
        [Route("Cart/GetCoupon")]
        public async Task<IActionResult> GetCoupon(string coupon_value)
        {
            // Kiểm tra coupon có tồn tại hay không
            var validCoupon = await _dataContext.Coupons
                .FirstOrDefaultAsync(x => x.Name == coupon_value);

            // Nếu không tìm thấy coupon
            if (validCoupon == null)
            {
                return Ok(new { success = false, message = "Coupon does not exist" });
            }

            // Tạo thông tin tiêu đề coupon
            string coupon_title = validCoupon.Name + " | " + validCoupon.Description;

            // Kiểm tra thời gian hết hạn
            TimeSpan remainingTime = validCoupon.DateExpired - DateTime.Now;
            int daysRemaining = remainingTime.Days;

            if (daysRemaining >= 0)
            {
                try
                {
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTimeOffset.UtcNow.AddMinutes(30),
                        Secure = true, // Sử dụng HTTPS
                        SameSite = SameSiteMode.Strict // Tăng cường bảo mật
                    };

                    // Lưu thông tin coupon vào cookie
                    Response.Cookies.Append("CouponTitle", coupon_title, cookieOptions);

                    return Ok(new { success = true, message = "Coupon applied successfully" });
                }
                catch (Exception e)
                {
                    // Ghi log lỗi và trả về thông báo lỗi
                    Console.WriteLine($"Error applying coupon: {e.Message}");
                    return Ok(new { success = false, message = "Failed to apply coupon" });
                }
            }
            else
            {
                // Coupon đã hết hạn
                return Ok(new { success = false, message = "Coupon has expired" });
            }
        }
    }
}