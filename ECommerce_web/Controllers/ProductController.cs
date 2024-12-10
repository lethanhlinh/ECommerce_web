using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
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

			var productsById = _dataContext.Products.Include(p=>p.Ratings).Where(p => p.Id == id).FirstOrDefault();
			
			var relatedProducts = await _dataContext.Products
				.Where(p => p.CategoryId == productsById.CategoryId && p.Id !=productsById.Id)
				.Take(4)
				.ToListAsync();
			// Truyền dữ liệu vào ViewModel
			var viewModel = new ProductDetailsViewModel
			{
				ProductDetails = productsById,
				Rating = productsById.Ratings // Truyền Rating duy nhất (vì quan hệ 1-1)
			};

			ViewBag.RelatedProducts = relatedProducts;

			return View(viewModel);
		}

		//Hàm comment
		public async Task<IActionResult> CommentProduct(RatingModel rating)
		{
			if (ModelState.IsValid)
			{
				var ratingEntity = new RatingModel
				{
					ProductId = rating.ProductId,
					Name = rating.Name,
					Email = rating.Email,
					Comment = rating.Comment,
					Star = rating.Star,

				};

				_dataContext.Ratings.Add(ratingEntity);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Thêm đánh giá thành công";

				// Chuyển hướng về trang Details để hiển thị bình luận
				return RedirectToAction("Details", new { id = rating.ProductId });
			}
			else
			{
				TempData["error"] = "Model có một vài thứ đang bị lỗi!!!";
				List<string> errors = new List<string>();
				foreach (var value in ModelState.Values)
				{
					foreach (var error in value.Errors)
					{
						errors.Add(error.ErrorMessage);
					}
				}
				string errorMessage = string.Join("\n", errors);
				
				return RedirectToAction("Details" , new {id=rating.ProductId});
			}
			// Chuyển hướng về trang Details để hiển thị bình luận
			return RedirectToAction("Details", new { id = rating.ProductId });
		}
	}
}
