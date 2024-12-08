using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce_web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Role")]
	[Authorize(Roles = "Admin")]
	public class RoleController : Controller
	{
		private readonly DataContext _dataContext;
		private readonly RoleManager<IdentityRole> _roleManager;
		public RoleController(DataContext dataContext, RoleManager<IdentityRole> roleManager)
		{
			_dataContext = dataContext;
			_roleManager = roleManager;
		}
		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Roles.OrderByDescending(r => r.Id).ToListAsync());
		}
		[HttpGet]
		[Route("Create")]
		public async Task<IActionResult> Create()
		{
			return View();
		}
		[HttpPost]
		[Route("Create")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(IdentityRole role)
		{
			//avoid duplicate role
			if (!_roleManager.RoleExistsAsync(role.Name).GetAwaiter().GetResult()) 
			{
				_roleManager.CreateAsync(new IdentityRole(role.Name)).GetAwaiter().GetResult();
                TempData["success"] = "Thêm Role thành công";
            }
			else
			{
				TempData["error"] = "Role đã tồn tại!!!";
			} 


			return Redirect("Index");
		}
		[HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var role = await _roleManager.FindByIdAsync(id);

			return View(role);
		}
		[HttpPost]
		[Route("Edit")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(string id, IdentityRole model)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			if (ModelState.IsValid)
			{
				var role = await _roleManager.FindByIdAsync(id);

				if (role == null)
				{
					return NotFound();
				}
				// Kiểm tra xem có thay đổi gì không
				if (role.Name == model.Name)
				{
					TempData["success"] = "Không có thay đổi nào được thực hiện!";
					return RedirectToAction("Index");  // Chuyển hướng về trang Index
				}

				// Nếu có thay đổi, cập nhật role
				role.Name = model.Name;

				try
				{
					var result = await _roleManager.UpdateAsync(role);

					if (result.Succeeded)
					{
						TempData["success"] = "Cập nhật Role thành công!!!";
						return RedirectToAction("Index");
					}
					else
					{
						foreach (var error in result.Errors)
						{
							ModelState.AddModelError("", error.Description);
						}
						TempData["error"] = "Cập nhật Role thất bại!!!";
					}
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Có lỗi khi thực hiện Update Role!!!");
				}
			}
			return View(model ?? new IdentityRole { Id = id });
		}
		[HttpGet]
		[Route("Delete")]
		public async Task<IActionResult> Delete(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var role = await _roleManager.FindByIdAsync(id);

			if (role == null)
			{
				return NotFound();
			}

			try
			{
				await _roleManager.DeleteAsync(role);
				TempData["success"] = "Xóa Role thành công !!!";
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "Lỗi khi thực hiện delete!!!");
			}
			return Redirect("Index");
		}
	}
}
