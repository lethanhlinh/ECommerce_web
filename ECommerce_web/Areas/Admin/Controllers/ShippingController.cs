using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Route("Admin/Shipping")]
    //[Authorize(Roles = "Publisher,Author, Admin")]
    public class ShippingController : Controller
    {
        private readonly DataContext _dataContext;
        public ShippingController(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        [Route("Index")]
        public IActionResult Index()
        {  
            return View();
        }
    }
}
