using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Route("Admin/Dashboard")]
    //[Authorize(Roles = "Admin,Author")]
    public class DashboardController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly IWebHostEnvironment _iwebHostEnviroment;
        public DashboardController(DataContext context, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = context;
            _iwebHostEnviroment = webHostEnvironment;
        }
        public IActionResult Index()
        {
           var count_product = _dataContext.Products.Count();
            var count_order = _dataContext.Orders.Count();
            var count_category = _dataContext.Categories.Count();
            var count_user = _dataContext.Users.Count();
            //Đếm sau đó đưa dl vào ViewBag
            ViewBag.CountProduct = count_product;
            ViewBag.CountOrder = count_order;
            ViewBag.CountCategory = count_category;
            ViewBag.CountUser = count_user;
            return View();
        }

        //Lấy dữ liệu để hiển thị lên Chart
        [HttpPost]
        public async Task<IActionResult> GetChartData()
        {
            var data = _dataContext.Statisticals.Select(s=> new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            }).ToList(); //Trả về mảng
            return Json(data); //Trả về Json 
        }
    }
}
