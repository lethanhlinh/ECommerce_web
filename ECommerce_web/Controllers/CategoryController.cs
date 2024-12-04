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
		public async Task<IActionResult> Index(string SLug = "", string sort_by="", string startprice ="", string endprice ="")
		{
			CategoryModel category = _dataContext.Categories.Where(c => c.Slug == SLug).FirstOrDefault();

			if (category == null)
			{
				return RedirectToAction("Index");
			}
			ViewBag.Slug = SLug;
            IQueryable<ProductModel> productsByCategory = _dataContext.Products.Where(p => p.CategoryId == category.Id);
            //lấy tất cả các sản phẩm
           
			var count = await productsByCategory.CountAsync();
			if (count > 0)
			{
				if (sort_by == "price_increase")
				{
					productsByCategory = productsByCategory.OrderBy(p => p.Price);
				}

				else if (sort_by == "price_decrease")
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Price);
				}

				else if (sort_by == "price_newest")
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
				}

				else if (sort_by == "price_oldest")
				{
					productsByCategory = productsByCategory.OrderBy(p => p.Id);
				}
				//lọc giá sản phẩm
				else if (startprice != "" && endprice !="")
				{
					decimal startPriceValue;
					decimal endPriceValue;

					if(decimal.TryParse(startprice, out startPriceValue) && decimal.TryParse(endprice, out endPriceValue))
					{
						productsByCategory=productsByCategory.Where(p => p.Price >= startPriceValue && p.Price <= endPriceValue);
					}	
					else
					{
						productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
					}	
				}	
				else
				{
					productsByCategory = productsByCategory.OrderByDescending(p => p.Id);
				} 
              }


            return View(await productsByCategory.ToListAsync());
		}
	}
}
