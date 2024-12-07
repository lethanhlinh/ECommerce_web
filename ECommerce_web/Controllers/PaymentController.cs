using ECommerce_web.Models;
using ECommerce_web.Services.Momo;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        public PaymentController (IMomoService momoService)
        {
            _momoService = momoService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrl(OrderInfoModel model)
        {
            var response = await _momoService.CreatePaymentAsync(model);
            return Redirect(response.PayUrl);
        }

        [HttpGet]
        public IActionResult PaymentCallback()
        {
            var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
            return View(response);
        } 
    }
}
