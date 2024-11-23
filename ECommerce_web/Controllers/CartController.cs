using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Controllers
{
	public class CartController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Checkout()
		{
			return View("~/Views/Checkout/Index.cshtml");
		}
	}
}
