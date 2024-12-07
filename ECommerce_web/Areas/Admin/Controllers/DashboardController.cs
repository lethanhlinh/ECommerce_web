using ECommerce_web.Repository;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        [Route("GetChartData")]
        public async Task<IActionResult> GetChartData()
        {
            var data = _dataContext.Statisticals.Select(s => new
            {
                date = s.DateCreated.ToString("yyyy-MM-dd"),
                sold = s.Sold,
                quantity = s.Quantity,
                revenue = s.Revenue,
                profit = s.Profit
            }).ToList(); //Trả về mảng
            return Json(data); //Trả về Json 
        }


        [HttpPost]
        [Route("GetChartDataBySelect")]
        [HttpPost]
        [Route("GetChartDataBySelect")]
        public IActionResult GetChartDataBySelect(DateTime startDate, DateTime endDate)
        {
            // Kiểm tra nếu ngày bắt đầu lớn hơn ngày kết thúc
            if (startDate == default || endDate == default || startDate > endDate)
            {
                return BadRequest("Ngày không hợp lệ.");
            }

            // Lấy dữ liệu từ cơ sở dữ liệu theo khoảng thời gian đã chọn
            var data = _dataContext.Statisticals
                .Where(s => s.DateCreated >= startDate && s.DateCreated <= endDate)
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data); // Trả về dữ liệu dưới dạng JSON
        }
        [HttpPost]
        [Route("FilterData")]
        public IActionResult FilterData(DateTime? fromDate, DateTime? toDate)
        {
            var query  = _dataContext.Statisticals.AsQueryable();
            if (fromDate.HasValue)
            {
                query = query.Where(s => s.DateCreated >= fromDate);
                
            }
            if (toDate.HasValue)
            {
                query = query.Where(s => s.DateCreated >= toDate);

            }
            var data = query
                .Select(s => new
                {
                    date = s.DateCreated.ToString("yyyy-MM-dd"),
                    sold = s.Sold,
                    quantity = s.Quantity,
                    revenue = s.Revenue,
                    profit = s.Profit
                })
                .ToList();

            return Json(data); // Trả về dữ liệu dưới dạng JSON
        }
    }
    }
