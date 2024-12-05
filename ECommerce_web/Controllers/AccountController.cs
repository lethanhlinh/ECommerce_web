using ECommerce_web.Areas.Admin.Repository;
using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
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
        public async Task<IActionResult> UpdateAccount()
        {
            if ((bool)!User.Identity?.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
        //get user by user email
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateInfoAccount(AppUserModel user)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            //var userEmail = User.FindFirstValue(ClaimTypes.Email);
            //get user by user email
            var userById = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userById == null)
            {
                return NotFound();
            }
            else
            {
                //Hash the new pasword
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(userById, user.PasswordHash);
                userById.PasswordHash = passwordHash;

                _dataContext.Update(userById);
                await _dataContext.SaveChangesAsync();
                TempData["success"] = "Update Account Information Successfully";
            }
            return RedirectToAction("UpdateAccount", "Account");
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
				var message = "Click on link to change password: " +
	                            $"<a href='{Request.Scheme}://{Request.Host}/Account/NewPass?email={checkMail.Email}&token={token}'>Reset Password</a>";


				await _emailSender.SendEmailAsync(receiver, subject, message);
            }

            TempData["success"] = "An email has been sent to your registered email address with password reset ints.";
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
            await HttpContext.SignOutAsync(); //Logout khi đăng nhập bằng tài khoản Google
            await _signInManager.SignOutAsync(); //Logout tài khoản thường
            return Redirect(returnUrl);
        }
        //Update password
        [HttpPost]
        public async Task<IActionResult> UpdateNewPassword(AppUserModel user, string token)
        {
            var check_user = await _userManager.Users
                .Where(u => u.Email == user.Email)
                .Where(u => u.Token == user.Token).FirstOrDefaultAsync();

            if(check_user != null)
            {
                //update user with new password and token
                string newtoken = Guid.NewGuid().ToString();
                //Hash the new pasword
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var passwordHash = passwordHasher.HashPassword(check_user, user.PasswordHash);

                check_user.PasswordHash = passwordHash;
                check_user.Token = newtoken;

                await _userManager.UpdateAsync(check_user);
                TempData["success"] = "Mật khẩu được đổi thành công!!!";
                return RedirectToAction("Login", "Account");

            }
            else
            {
                TempData["error"] = "Không tìm thấy Email hoặc Token không tồn tại";
                return RedirectToAction("ForgetPass", "Account");
            }
            return View();
        }

        //Đăng nhập bằng Google
        public async Task LoginByGoogle()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });
        }
        //Xử lí khi đăng nhập bằng tài khoản Google
        public async Task<IActionResult> GoogleResponse()
        {
            //var result = await HttpContext
            //    .AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(Claim => new
            //{
            //    Claim.Issuer,
            //    Claim.OriginalIssuer,
            //    Claim.Type,
            //    Claim.Value
            //});
            // TempData["success"] = "Đăng nhập tài khoản Google thành công";
            // return RedirectToAction("Index", "Home");
            //return Json(claims);
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if(!result.Succeeded)
            {
                //Nếu xác thực ko thành công quay về trang Login
                return RedirectToAction("Login");
            }
            var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
            { //claim: mọi thông tin khi đăng nhập thành công
                claim.Issuer,
                claim.OriginalIssuer,
                claim.Type,
                claim.Value
            });
          //  return Json(claims);
          var email = claims.FirstOrDefault(c=>c.Type == ClaimTypes.Email)?.Value;
            //var name = claims.FirstOrDefault(c=>c.Type== ClaimTypes.GivenName)?.Value; : ko tối ưu khi có dấu
            string emailName = email.Split('@')[0];  //phân tách tên email dựa vào dấu @

            var existingUser = await _userManager.FindByEmailAsync(email); //Check sự tồn tại của user

            if(existingUser == null) //ko tồn tại, tạo user mới
            {
                //nếu user ko tồn tại trong db thì tạo user mới với password mặc định 1-9
                var passwordHasher = new PasswordHasher<AppUserModel>();
                var hashedPassword = passwordHasher.HashPassword(null, "123456789");

                //Tạo user mới
                var newUser = new AppUserModel
                {
                    UserName = emailName,
                    Email = email
                };
                newUser.PasswordHash = hashedPassword;
                //Tạo user
                var createUserResult = await _userManager.CreateAsync(newUser);
                if (!createUserResult.Succeeded) 
                {
                    TempData["error"] = "Đăng kí tài khoản thất bại. Vui lòng thử lại";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    //Nếu tạo user thành công thì đăng nhập
                    await _signInManager.SignInAsync(newUser, isPersistent: false);
                    TempData["success"] = "Đăng kí tài khoản thành công";
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {
                //đã có user thì đăng nhập với existingUser
                await _signInManager.SignInAsync(existingUser, isPersistent: false);
            }
            return RedirectToAction("Login","Account");
        }
	}
}
