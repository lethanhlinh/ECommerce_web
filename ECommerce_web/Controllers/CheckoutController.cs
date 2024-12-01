using ECommerce_web.Areas.Admin.Repository;
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
        private readonly IEmailSender _emailSender;
        public CheckoutController(IEmailSender emailSender, DataContext context)
        {
            _dataContext = context;
            _emailSender = emailSender;
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
                var ordercode = Guid.NewGuid().ToString();
                var orderItem = new OrderModel();
                orderItem.UserName = userEmail;
                orderItem.OrderCode = ordercode;
                orderItem.Status = 1;
                orderItem.CreateDate = DateTime.Now;

                _dataContext.Add(orderItem);
                _dataContext.SaveChanges();

                List<CartItemModel> cartItems = HttpContext.Session.GetJson<List<CartItemModel>>("Cart") ?? new List<CartItemModel>();
                foreach (var cart in cartItems)
                {
                    var orderdetails = new OrderDetails();
                    orderdetails.UserName = userEmail;
                    orderdetails.OrderCode = ordercode;
                    orderdetails.ProductId = cart.ProductId;
                    orderdetails.Price =cart.Price;
                    orderdetails.Quantity =cart.Quantity;

                    _dataContext.Add(orderdetails);
                    _dataContext.SaveChanges();
                }
                HttpContext.Session.Remove("Cart");
                //Gửi mail khi đặt hàng thành công
                var receiver = userEmail; // Người nhận
                var subject = "Đặt hàng thành công"; //Tiêu đề
                var message = "Đặt hàng thành công ! Cảm ơn bạn rất nhiều"; //Nội dung

                await _emailSender.SendEmailAsync(receiver, subject, message);

                TempData["success"] = "Đơn hàng đã được tạo thành công, vui lòng chờ duyệt đơn hàng";
                return RedirectToAction("Index", "Cart");
            }
            return View();
        }
    }
}
