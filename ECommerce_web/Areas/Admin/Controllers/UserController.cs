using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ECommerce_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/User")]
    //[Authorize(Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUserModel> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _dataContext;
        public UserController(DataContext context, UserManager<AppUserModel> userManager, RoleManager<IdentityRole> roleManager)
        {
           
            _userManager = userManager;
            _roleManager = roleManager;
            _dataContext = context;
        }
        [HttpGet]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var usersWithRoles = await (from u in _dataContext.Users
                                       join ur in _dataContext.UserRoles on u.Id equals ur.UserId
                                       join r in _dataContext.Roles on ur.RoleId equals r.Id
                                       select new { User = u, RoleName = r.Name })
                                       .ToListAsync();
            
            return View(usersWithRoles);
           // return View(await _userManager.Users.OrderByDescending(c => c.Id).ToListAsync());
            
        }

		[HttpGet]
		[Route("Create")]
		public async Task<IActionResult> Create()
		{
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");

			return View(new AppUserModel());
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		[Route("Create")]
		public async Task<IActionResult> Create(AppUserModel user)
		{
			if (ModelState.IsValid)
			{
				var createUserResult = await _userManager.CreateAsync(user, user.PasswordHash);
				if (createUserResult.Succeeded)
				{
					var createUser = await _userManager.FindByEmailAsync(user.Email);
					var userId = createUser.Id;
					var role = await _roleManager.FindByIdAsync(user.RoleId);
					var addToRoleResult = await _userManager.AddToRoleAsync(createUser, role.Name);

					if (!addToRoleResult.Succeeded)
					{
						foreach (var error in addToRoleResult.Errors) // Đổi từ createUserResult.Errors thành addToRoleResult.Errors
						{
							ModelState.AddModelError(string.Empty, error.Description);
						}
					}
					return RedirectToAction("Index", "User");
				}
				else
				{
					// Nạp lại danh sách Role khi có lỗi
					var roles = await _roleManager.Roles.ToListAsync();
					ViewBag.Roles = new SelectList(roles, "Id", "Name");

					// Hiển thị lỗi cho người dùng
					foreach (var error in createUserResult.Errors)
					{
						ModelState.AddModelError(string.Empty, error.Description);
					}
					return View(user);
				}
			}
			else
			{
				// Nạp lại danh sách Role nếu ModelState không hợp lệ
				var roles = await _roleManager.Roles.ToListAsync();
				ViewBag.Roles = new SelectList(roles, "Id", "Name");

				TempData["error"] = "Model có một vài thứ đang bị lỗi!!!";
				return View(user);
			}
		}


		[HttpGet]
        [Route("Delete")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
             return NotFound();
            }
             var user = await _userManager.FindByNameAsync(id);
              if (user == null) 
              {
               return NotFound();
              }
                var deleteResult = await _userManager.DeleteAsync(user);
                   if (!deleteResult.Succeeded)
                   {
                    return View("Error");
                   }
                  TempData["success"] = "Đã xóa user thành công!!!";
                  return RedirectToAction("Index");
        }

		[HttpGet]
		[Route("Edit")]
		public async Task<IActionResult> Edit(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return NotFound();
			}
			var user = await _userManager.FindByNameAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			var roles = await _roleManager.Roles.ToListAsync();
			ViewBag.Roles = new SelectList(roles, "Id", "Name");

			return View(user);
		}

        [HttpPost]
		[Route("Edit")]
		[ValidateAntiForgeryToken]
       
        public async Task<IActionResult> Edit(string id, AppUserModel user)
        {
            var existingUser = await _userManager.FindByNameAsync(id); //lấu user dựa theo Username

            if (existingUser == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                existingUser.UserName = user.UserName;
                existingUser.Email = user.Email;
                existingUser.PhoneNumber = user.PhoneNumber;
                existingUser.RoleId = user.RoleId;

                var updateUserResult = await _userManager.UpdateAsync(existingUser); //thực hiện việc update
                if (updateUserResult.Succeeded)
                {
                    return RedirectToAction("Index", "User");
                }
                else
                {
                    AddIdentityError(updateUserResult);
                    return View(existingUser);
                }    

            }
            var roles = await _roleManager.Roles.ToListAsync();
            ViewBag.Roles = new SelectList(roles, "Id", "Name");

            //Model validation failed
            TempData["error"] = "Model validation failed";
            var error = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList();
           
            return View(existingUser);
        }
		private void AddIdentityError(IdentityResult identityResult) //Hàm để hiển thị lỗi
		{
			foreach (var error in identityResult.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}
		}
	}
 }

