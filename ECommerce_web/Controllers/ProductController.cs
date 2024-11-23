using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Controllers
{
	public class ProductController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult Details()
		{
			return View();
		}
	}
}
