using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Controllers
{
	public class CategoryController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}
