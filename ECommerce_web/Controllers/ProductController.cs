using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Controllers
{
	public class ProductController : Controller
	{
		private readonly DataContext _dataContext;
		public ProductController(DataContext context)
		{
			_dataContext = context;
		}
		public IActionResult Index()
		{
			return View();
		}
        public async Task<IActionResult> Search(string searchTerm)
		{
			var products = await _dataContext.Products
			.Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
				.ToListAsync();
			ViewBag.Keyword = searchTerm;
			return View(products);
		}

        public async Task<IActionResult> Details(long id)
		{
			if(id == null) RedirectToAction("Index");

			var productsById = _dataContext.Products.Where(p => p.Id == id).FirstOrDefault();
			
			return View(productsById);
		}
	}
}
