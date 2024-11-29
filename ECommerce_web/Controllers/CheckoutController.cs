using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce_web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly DataContext _dataContext;
        public CheckoutController(DataContext context)
        {
            _dataContext = context;
        }

        public async Task<IActionResult> Checkout()
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            if(userEmail == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var orderCode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.UserName = userEmail;
                orderItem.OrderCode = orderCode;
                orderItem.Status = 1;
                orderItem.CreateDate = DateTime.Now;

                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                TempData["success"] = "Đơn hàng đã được đặt";
                return RedirectToAction("Index", "Cart");
            }
            return View();
        }
    }
}
