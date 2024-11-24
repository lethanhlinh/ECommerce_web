using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Controllers
{
	public class CategoryController : Controller
	{
		private DataContext _dataContext;
		public CategoryController(DataContext context)
		{
			_dataContext = context;
		}
		public async Task<IActionResult> Index(string SLug = "")
		{
			CategoryModel category = _dataContext.Categories.Where(c => c.Slug == SLug).FirstOrDefault();
			
			if (category == null) return RedirectToAction("Index");
			
			var productsByCategory = _dataContext.Products.Where(p =>  p.CategoryId == category.Id);

			return View(await productsByCategory.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}
