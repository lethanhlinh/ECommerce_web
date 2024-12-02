using ECommerce_web.Repository;
using ECommerce_web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerce_web.Areas.Admin.Controllers
{

	[Area("Admin")]
	//[Route("Admin/Slider")]
	//[Authorize(Roles = "Publisher")]
	public class SliderController : Controller
	{
        private readonly IWebHostEnvironment _iwebHostEnviroment;
        private readonly DataContext _dataContext;
		public SliderController(DataContext dataContext, IWebHostEnvironment webHostEnvironment)
		{
			_dataContext = dataContext;
            _iwebHostEnviroment = webHostEnvironment;
        }

	//	[Route("Index")]
		public async Task<IActionResult> Index()
		{
            return View(await _dataContext.Sliders.OrderByDescending(c => c.Id).ToListAsync());
        }
		public IActionResult Create()
		{
			return View();
		}
        [HttpPost]
        //[Route("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderModel slider)
        {
          

            if (ModelState.IsValid)
            {
                // code thêm dữ liệu
            
                
          
                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();

                    slider.Image = imageName;
                }
                _dataContext.Add(slider);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Thêm slider thành công!!!";
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

            return View(slider);
        }
        [HttpGet]
        //[Route("Edit")]
        public async Task<IActionResult> Edit(int Id)
        {
            SliderModel slider = await _dataContext.Sliders.FindAsync(Id);
          
            return View(slider);
        }
        [HttpPost]
        //[Route("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SliderModel slider)
        {

            var slider_existed = _dataContext.Sliders.Find(slider.Id);
            if (ModelState.IsValid)
            {
                // code thêm dữ liệu



                if (slider.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/sliders");
                    string imageName = Guid.NewGuid().ToString() + "_" + slider.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await slider.ImageUpload.CopyToAsync(fs);
                    fs.Close();

                    slider_existed.Image = imageName;
                }

                slider_existed.Name = slider.Name;
                slider_existed.Description = slider.Description;
                slider_existed.Status = slider.Status;
              
                _dataContext.Update(slider_existed);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Cập nhật slider thành công!!!";
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

            return View(slider);
        }
        //[Route("Delete")]
        [HttpGet]
        public async Task<IActionResult> Delete(int Id)
        {
                SliderModel slider = await _dataContext.Sliders.FindAsync(Id);

          
                string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/sliders");
                string oldfileImage = Path.Combine(uploadsDir, slider.Image);

               
                    if (System.IO.File.Exists(oldfileImage))
                    {
                        System.IO.File.Delete(oldfileImage);
                    }
                
               
            _dataContext.Remove(slider);
            await _dataContext.SaveChangesAsync();
            TempData["error"] = "Slider đã được xóa!!!";
            return RedirectToAction("Index");
        }
    }
}
