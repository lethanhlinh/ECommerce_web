using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Areas.Admin.Controllers
{
    [Area("Admin")]
	//[Route("Admin/Product")]
	//[Authorize(Roles ="Admin")]
	public class ProductController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _iwebHostEnviroment;
        public ProductController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _iwebHostEnviroment = webHostEnvironment;
        }
		//[Route("Index")]
        public async Task<IActionResult> Index()
        {
            return View(await _dataContext.Products.OrderByDescending(p => p.Id).Include(p => p.Category).Include(p => p.Brand).ToListAsync());
        }
        [HttpGet]
		//[Route("Create")]
		public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            return View();
        }
        [HttpPost]
		//[Route("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ProductModel product)
		{
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			if (ModelState.IsValid)
			{
				// code thêm dữ liệu
				product.Slug = product.Name.Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug);
				if (slug != null)
				{
					ModelState.AddModelError("", "Sản phẩm đã tồn tại!!!");
					return View(product);
				}
				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					FileStream fs = new FileStream(filePath, FileMode.Create);
					await product.ImageUpload.CopyToAsync(fs);
					fs.Close();

					product.Image = imageName;
				}
				_dataContext.Add(product);
				await _dataContext.SaveChangesAsync();
				TempData["success"] = "Thêm sản phẩm thành công!!!";
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

            return View(product);
		}
		[HttpGet]
		//[Route("Edit")]
        public async Task<IActionResult> Edit(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);
            ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name");
            ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name");
            
            return View(product);
        }
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(ProductModel product)
		{
			// Tạo dropdown list cho Category và Brand
			ViewBag.Categories = new SelectList(_dataContext.Categories, "Id", "Name", product.CategoryId);
			ViewBag.Brands = new SelectList(_dataContext.Brands, "Id", "Name", product.BrandId);

			// Kiểm tra xem sản phẩm có tồn tại trong cơ sở dữ liệu không
			var existingProduct = await _dataContext.Products.FindAsync(product.Id);
			if (existingProduct == null)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				// Kiểm tra trùng slug
				product.Slug = product.Name.Replace(" ", "-");
				var slug = await _dataContext.Products.FirstOrDefaultAsync(p => p.Slug == product.Slug && p.Id != product.Id);
				if (slug != null)
				{
					ModelState.AddModelError("", "Sản phẩm với tên này đã tồn tại!!!");
					return View(product);
				}

				// Xử lý ảnh nếu có ảnh mới
				if (product.ImageUpload != null)
				{
					string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/products");
					string imageName = Guid.NewGuid().ToString() + "_" + product.ImageUpload.FileName;
					string filePath = Path.Combine(uploadsDir, imageName);

					// Xóa ảnh cũ nếu có
					string oldfileImage = Path.Combine(uploadsDir, existingProduct.Image);
					try
					{
						if (System.IO.File.Exists(oldfileImage) && !string.Equals(existingProduct.Image, "noname.jpg"))
						{
							System.IO.File.Delete(oldfileImage);
						}
					}
					catch (Exception ex)
					{
						ModelState.AddModelError("", "Có lỗi xảy ra khi xóa ảnh sản phẩm cũ.");
						return View(product);
					}

					// Lưu ảnh mới
					using (FileStream fs = new FileStream(filePath, FileMode.Create))
					{
						await product.ImageUpload.CopyToAsync(fs);
					}

					existingProduct.Image = imageName;
				}

				// Cập nhật các thuộc tính khác của sản phẩm
				existingProduct.Name = product.Name;
				existingProduct.Description = product.Description;
                existingProduct.Price = product.CapitalPrice;
                existingProduct.Price = product.Price;
				existingProduct.CategoryId = product.CategoryId;
				existingProduct.BrandId = product.BrandId;

				// Cập nhật sản phẩm trong cơ sở dữ liệu
				_dataContext.Update(existingProduct);
				await _dataContext.SaveChangesAsync();

				TempData["success"] = "Cập nhật sản phẩm thành công!";
				return RedirectToAction("Index");
			}

			TempData["error"] = "Thông tin sản phẩm không hợp lệ!";
			return View(product);
		}
		//[Route("Delete")]
		public async Task<IActionResult> Delete(long Id)
        {
            ProductModel product = await _dataContext.Products.FindAsync(Id);

            if (!string.Equals(product.Image, "noname.jpg"))
            {
                string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/products");
                string oldfileImage = Path.Combine(uploadsDir, product.Image);

                try
                {
                    if (System.IO.File.Exists(oldfileImage))
                    {
                        System.IO.File.Delete(oldfileImage);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while deleting the product image.");
                }
            }
            _dataContext.Remove(product);
            await _dataContext.SaveChangesAsync();
            TempData["success"] = "Sản phẩm đã được xóa!!!";
            return RedirectToAction("Index","Product");
        }

        //Add more Quantity to products
		
		[Route("AddQuantity")]
		[HttpGet]
        public async Task<IActionResult> AddQuantity(int Id)
        {
			var productbyquantity = await _dataContext.ProductQuantities.Where(pq => pq.ProductId == Id).ToListAsync();
			ViewBag.ProductByQuantity = productbyquantity;
			ViewBag.Id = Id;
            return View();
        }

		[Route("StoreProductQuantity")]
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult StoreProductQuantity(ProductQuantityModel productQuantityModel)
		{
			//Get the product to update
			var product = _dataContext.Products.Find(productQuantityModel.ProductId); //Tìm sp dựa theo product ID

			if(product == null)
			{
				return NotFound();
			}
			product.Quantity += productQuantityModel.Quantity;

			productQuantityModel.Quantity = productQuantityModel.Quantity; //Cộng dồn số lượng
			productQuantityModel.ProductId = productQuantityModel.ProductId; 
			productQuantityModel.DateCreated = DateTime.Now;


			_dataContext.Add(productQuantityModel);
			_dataContext.SaveChangesAsync();
			TempData["success"] = "Thêm số lượng sản phẩm thành công";
			//return RedirectToAction("AddQuantity", "Product", new { Id = productQuantityModel.ProductId });
			return RedirectToAction("Index", "Product");//Chuyển hướng về lại trang Index
		}
    }
}
