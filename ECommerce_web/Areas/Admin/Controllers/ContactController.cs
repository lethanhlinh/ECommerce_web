using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Admin/Brand")]
    //[Authorize(Roles = "Admin,Author")]
    public class ContactController : Controller
    {
    private readonly DataContext _dataContext;
    private readonly IWebHostEnvironment _iwebHostEnviroment;
        public ContactController(DataContext dataContext , IWebHostEnvironment iwebHostEnviroment)
        {
            _dataContext = dataContext;
            _iwebHostEnviroment = iwebHostEnviroment;
        }
        [Route("Index")]
        public IActionResult Index()
        {
            var contact = _dataContext.Contact.ToList();
            return View(contact);
        }
        [Route("Edit")]
        public async Task<IActionResult> Edit()
        {
            ContactModel contact = await _dataContext.Contact.FirstOrDefaultAsync();
            return View(contact);
        }
        [Route("Edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ContactModel contact)
        {
            var existed_contact = _dataContext.Contact.FirstOrDefault();

            if (ModelState.IsValid)
            {

                if (contact.ImageUpload != null)
                {
                    string uploadsDir = Path.Combine(_iwebHostEnviroment.WebRootPath, "media/logo");
                    string imageName = Guid.NewGuid().ToString() + "_" + contact.ImageUpload.FileName;
                    string filePath = Path.Combine(uploadsDir, imageName);

                    FileStream fs = new FileStream(filePath, FileMode.Create);
                    await contact.ImageUpload.CopyToAsync(fs);
                    fs.Close();
                    existed_contact.LogoImg = imageName;


                }

                // Cập nhật các thuộc tính khác của sản phẩm
                existed_contact.Name = contact.Name;
                existed_contact.Email = contact.Email;
                existed_contact.Description = contact.Description;
                existed_contact.Phone = contact.Phone;
                existed_contact.Map = contact.Map;


                // Cập nhật sản phẩm trong cơ sở dữ liệu
                _dataContext.Update(existed_contact);
                await _dataContext.SaveChangesAsync();

                TempData["success"] = "Cập nhật thông tin web thành công!";
                return RedirectToAction("Index");
            }

            TempData["error"] = "Còn một số lỗi!";
            return View(contact);
        }
    }
}
