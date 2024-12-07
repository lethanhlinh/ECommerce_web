using ECommerce_web.Models;
using ECommerce_web.Models.Vnpay;
using ECommerce_web.Services.Momo;
using ECommerce_web.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Controllers
{
    public class PaymentController : Controller
    {
        private IMomoService _momoService;
        private readonly IVnPayService _vnPayService;
        public PaymentController (IMomoService momoService, IVnPayService vnPayService)
        {
            _momoService = momoService;
            _vnPayService = vnPayService;
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

        //Vnpay


        [HttpPost]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            return Redirect(url);
        }


    


}
}
