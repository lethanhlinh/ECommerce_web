using ECommerce_web.Areas.Admin.Repository;
using ECommerce_web.Models;
using ECommerce_web.Models.ViewModels;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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


				//Nhận shippingPrice từ cookie
				var shippingPriceCookie = Request.Cookies["ShippingPrice"];
				decimal shippingPrice = 0;

                //Nhận shippingPrice từ cookie
                var coupon_code = Request.Cookies["CouponTitle"];

                if (shippingPriceCookie != null)
				{
					var shippingPriceJson = shippingPriceCookie;
					shippingPrice = JsonConvert.DeserializeObject<decimal>(shippingPriceJson);
				}

				orderItem.ShippingCost = shippingPrice;
                orderItem.CouponCode = coupon_code;
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
                    //update product quantity
                    var product = await _dataContext.Products.Where(p => p.Id == cart.ProductId).FirstAsync();
                    product.Quantity -= cart.Quantity;
                    product.Sold += cart.Quantity;
                    _dataContext.Update(product);
                    //++update product quantity
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
                return RedirectToAction("History", "Account");
            }
            return View();
        }
    }
}
