using ECommerce_web.Areas.Admin.Repository;
using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        public AccountController(IEmailSender emailSender, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnURL = returnUrl});
        }
		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginVM)
        {
            if (ModelState.IsValid)
            {
                Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(loginVM.Username, loginVM.Password, false, false);
                if (result.Succeeded)
                {
                    return Redirect(loginVM.ReturnURL ?? "/");
                }
                ModelState.AddModelError("", "Username hoặc Password bị lỗi.");
            }
            return View(loginVM);
        }
        [HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(UserModel user)
		{
            if (ModelState.IsValid)
            {
                AppUserModel newUser = new AppUserModel { UserName = user.Username, Email = user.Email};
                IdentityResult result = await _userManager.CreateAsync(newUser, user.Password);
                if (result.Succeeded)
                {
                    TempData["success"] = "Tạo tài khoản thành công";
                    return Redirect("/Account/Login");
                }
				foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

			}
			return View(user);
		}
		public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await _signInManager.SignOutAsync();
            return Redirect(returnUrl);
        }
	}
}
