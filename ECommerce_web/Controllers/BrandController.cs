using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Controllers
{
	public class BrandController : Controller
	{
		private readonly DataContext _dataContext;
		public BrandController(DataContext context) {
			_dataContext = context;
		}
		public async Task<IActionResult> Index(string SLug = "")
		{
			BrandModel brand = _dataContext.Brands.Where(b => b.Slug == SLug).FirstOrDefault();

			if (brand == null) return RedirectToAction("Index");

			var productsByBrand = _dataContext.Products.Where(p => p.BrandId == brand.Id);

			return View(await productsByBrand.OrderByDescending(p => p.Id).ToListAsync());
		}
	}
}
