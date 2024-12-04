using ECommerce_web.Areas.Admin.Repository;
using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SqlServer.Server;
using Microsoft.VisualBasic;
using System.Security.Claims;

namespace ECommerce_web.Controllers
{
    public class AccountController : Controller
    {
        private UserManager<AppUserModel> _userManager;
        private SignInManager<AppUserModel> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly DataContext _dataContext;
        public AccountController(IEmailSender emailSender, UserManager<AppUserModel> userManager, SignInManager<AppUserModel> signInManager, DataContext context)
        {
            _dataContext = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _emailSender = emailSender;
        }
        [HttpGet]
        public IActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel { ReturnURL = returnUrl});
        }



        public async Task<IActionResult> NewPass(AppUserModel user, string token)
        {
            var checkuser = await _userManager.Users
            .Where(u => u.Email == user.Email)
            .Where(u => u.Token == user.Token).FirstOrDefaultAsync();
            if (checkuser != null)
            {
                ViewBag.Email = checkuser.Email;
                ViewBag.Token = token;
            }
            else
            {
                TempData["error"] = "Email not found or token is not right";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }

			public async Task<IActionResult> SendMailForgotPass(AppUserModel user)
        {
            var checkMail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == user.Email);
            if (checkMail == null)
            {
                TempData["error"] = "Email not found";
                return RedirectToAction("ForgetPass", "Account");
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                //update token to user
                checkMail.Token = token;
                _dataContext.Update(checkMail);
                await _dataContext.SaveChangesAsync();
                var receiver = checkMail.Email;
                var subject = "Change password for user " + checkMail.Email;
                var message = "Click on link to change password" +
                    "<a href='" + $"{Request.Scheme}://{Request.Host}/Account/NewPass? email=" +
                    checkMail.Email + "&token=" + token + "'>";

                await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "An email has been sent to your registered email address with password reset ints,";
            return RedirectToAction("ForgetPass", "Account");
        }  

		public async Task<IActionResult> ForgetPass(string returnUrl)
		{
			return View();
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

        public async Task<IActionResult> History()
        {
            if((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId =User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var Orders = await _dataContext.Orders
                .Where(od => od.UserName == userEmail).OrderByDescending(od => od.Id).ToListAsync();
            ViewBag.UserEmail=userEmail;
            return View(Orders);
        }

        public async Task<IActionResult> CancelOrder(string ordercode)
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");

            }
            try
            {
                var order = await _dataContext.Orders.Where(o => o.OrderCode == ordercode).OrderByDescending(od => od.Id).FirstAsync();
                order.Status = 3;
                _dataContext.Update(order);
                await _dataContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest("An error occurred while canceling the order.");
            }
            return RedirectToAction("History", "Account");
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
