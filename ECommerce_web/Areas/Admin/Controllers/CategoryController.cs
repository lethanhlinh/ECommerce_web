using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Areas.Admin.Controllers
{
	[Area("Admin")]
	//[Route("Admin/Category")]
	//[Authorize("Admin, Author")]
	public class CategoryController : Controller 
	{
		private readonly DataContext _dataContext;
		public CategoryController(DataContext dataContext) { 
			_dataContext = dataContext;
		}
        //public async Task<IActionResult> Index()
        //{
        //	return View(await _dataContext.Categories.OrderByDescending(c => c.Id).ToListAsync());
        //}

        //Phân trang
    //    [Route("Index")]
        public async Task<IActionResult> Index(int pg = 1)
        {
            List<CategoryModel> category = _dataContext.Categories.ToList(); //33 datas


            const int pageSize = 10; //10 items/trang

            if (pg < 1) //page < 1;
            {
                pg = 1; //page ==1
            }
            int recsCount = category.Count(); //33 items;đếm

            var pager = new Paginate(recsCount, pg, pageSize);

            int recSkip = (pg - 1) * pageSize; //(3 - 1) * 10; 

            //category.Skip(20).Take(10).ToList()

            var data = category.Skip(recSkip).Take(pager.PageSize).ToList();

            ViewBag.Pager = pager;

            return View(data);
        }

        [HttpGet]
		//[Route("Create")]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		//[Route("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CategoryModel category)
		{

			if (ModelState.IsValid)
			{
				// code thêm dữ liệu
				category.Slug = category.Name.Replace(" ", "-");
				var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Danh mục đã tồn tại!!!");
					return View(category);
				}
				
				_dataContext.Add(category);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm danh mục thành công!!!";
				return RedirectToAction("Index");
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
				return BadRequest(errorMessage);
			}

			return View(category);
		}

		//Edit danh mục
		[HttpGet]
		//[Route("Edit")]
		public async Task<IActionResult> Edit(int Id)
		{
			CategoryModel category = await _dataContext.Categories.FindAsync(Id);
			return View(category);
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(CategoryModel category)
		{
			if (ModelState.IsValid)
			{
				// Lấy danh mục hiện tại từ cơ sở dữ liệu
				var existingCategory = await _dataContext.Categories.FindAsync(category.Id);

				if (existingCategory == null)
				{
					return NotFound();
				}

				// Kiểm tra nếu tên danh mục (Name) có thay đổi không
				if (existingCategory.Name != category.Name)
				{
					// Nếu tên thay đổi, tạo lại Slug và kiểm tra trùng lặp
					category.Slug = category.Name.Replace(" ", "-");
					var slug = await _dataContext.Categories.FirstOrDefaultAsync(c => c.Slug == category.Slug);
					if (slug != null)
					{
						ModelState.AddModelError("", "Danh mục đã tồn tại!!!");
						return View(category);
					}
				}
				else
				{
					// Nếu chỉ thay đổi mô tả, không cần thay đổi slug
					category.Slug = existingCategory.Slug;
				}

				// Đảm bảo không có entity trùng lặp
				_dataContext.Entry(existingCategory).State = EntityState.Detached;

				// Gắn category vào DbContext
				_dataContext.Update(category);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật danh mục thành công!!!";
				return RedirectToAction("Index");
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
				return BadRequest(errorMessage);
			}

			return View(category);
		}

		//[Route("Delete")]
		public async Task<IActionResult> Delete(int Id)
		{
			CategoryModel category = await _dataContext.Categories.FindAsync(Id);

			
			_dataContext.Remove(category);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Danh mục đã được xóa!!!";
			return RedirectToAction("Index");
		}
	}
}
