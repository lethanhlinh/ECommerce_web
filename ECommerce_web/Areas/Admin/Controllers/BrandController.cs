using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Areas.Admin.Controllers
{
    [Area("Admin")]
	[Authorize]
	public class BrandController : Controller
    {

        private readonly DataContext _dataContext;
        public BrandController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Brands.OrderByDescending(c => c.Id).ToListAsync());
        }

		public async Task<IActionResult> Create()
        {
            return View();
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(BrandModel brand)
		{

			if (ModelState.IsValid)
			{
				// code thêm dữ liệu
				brand.Slug = brand.Name.Replace(" ", "-");
				var slug = await _dataContext.Brands.FirstOrDefaultAsync(c => c.Slug == brand.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Thương hiệu đã tồn tại!!!");
					return View(brand);
				}

				_dataContext.Add(brand);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm thương hiệu thành công!!!";
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

			return View(brand);
		}

		public async Task<IActionResult> Delete(int Id)
		{
			BrandModel brand = await _dataContext.Brands.FindAsync(Id);


			_dataContext.Brands.Remove(brand);
			await _dataContext.SaveChangesAsync();
			TempData["success"] = "Thywowng hiệu đã được xóa!!!";
			return RedirectToAction("Index");
		}

		//Edit danh mục
		[HttpGet]
		public async Task<IActionResult> Edit(int Id)
		{
			BrandModel brand = await _dataContext.Brands.FindAsync(Id);
			return View(brand);
		}


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(BrandModel brand)
		{

			if (ModelState.IsValid)
			{
                // Lấy danh mục hiện tại từ cơ sở dữ liệu
                var existingBrand = await _dataContext.Brands.FindAsync(brand.Id);

                if (existingBrand == null)
                {
                    return NotFound();
                }

                // Kiểm tra nếu tên danh mục (Name) có thay đổi không
                if (existingBrand.Name != brand.Name)
                {
                    // Nếu tên thay đổi, tạo lại Slug và kiểm tra trùng lặp
                    brand.Slug = brand.Name.Replace(" ", "-");
                    var slug = await _dataContext.Brands.FirstOrDefaultAsync(b => b.Slug == brand.Slug);
                    if (slug != null)
                    {
                        ModelState.AddModelError("", "Danh mục đã tồn tại!!!");
                        return View(brand);
                    }
                }
                else
                {
                    // Nếu chỉ thay đổi mô tả, không cần thay đổi slug
                    brand.Slug = existingBrand.Slug;
                }

                // Đảm bảo không có entity trùng lặp
                _dataContext.Entry(existingBrand).State = EntityState.Detached;

                _dataContext.Update(brand);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Cập nhật thương hiệu thành công!!!";
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

			return View(brand);
		}

	}



}
