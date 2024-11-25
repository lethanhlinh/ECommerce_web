using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
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
			CartItemViewModel cartVM = new()
			{
				CartItems = cartItems,
				GrandTotal = cartItems.Sum(x => x.Quantity * x.Price)
			};
			return View(cartVM);
		}
		public IActionResult Checkout()
		{
			return View("~/Views/Checkout/Index.cshtml");
		}
		public async Task<IActionResult> Add(int Id)
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
		public async Task<IActionResult> Decrease(int Id)
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
		public async Task<IActionResult> Increase(int Id)
		{
			List<CartItemModel> cart = HttpContext.Session.GetJson<List<CartItemModel>>("Cart");

			CartItemModel cartItem = cart.Where(c => c.ProductId == Id).FirstOrDefault();
			// lay san pham bang id cua san pham can giam so luong

			if (cartItem.Quantity >= 1)
			{
				++cartItem.Quantity;
				// tang so luong 
			}
			//else
			//{
			//	cart.RemoveAll(p => p.ProductId == Id);
			//	// xoa luon san pham do
			//}
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
		public async Task<IActionResult> Clear(int id)
		{
			//xoa het gio hang
			HttpContext.Session.Remove("Cart");
            TempData["success"] = "Clear all Item of cart Successfully! ";
            return RedirectToAction("Index");
		}
	}
	

}
