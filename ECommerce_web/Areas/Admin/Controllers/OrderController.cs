using ECommerce_web.Models;
using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;

namespace ECommerce_web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Route("Admin/Order")]
	//[Authorize("Admin,Author")]
	public class OrderController : Controller
	{
		private readonly DataContext _dataContext;
		public OrderController(DataContext context)
		{
			_dataContext = context;
		}
		[Route("Index")]
		public async Task<IActionResult> Index()
		{
			return View(await _dataContext.Orders.OrderByDescending(c => c.Id).ToListAsync());
		}


        [HttpGet]
        [Route("ViewOrder")]
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od=> od.Product)
				.Where(od => od.OrderCode==ordercode).ToListAsync();
			var Order = _dataContext.Orders.Where( o => o.OrderCode == ordercode).First();
			ViewBag.ShippingCost= Order.ShippingCost;
			ViewBag.Status = Order.Status;
			return View(DetailsOrder);
		}


        [HttpPost]
		[Route("UpdateOrder")]
        public async Task<ActionResult> UpdateOrder(string ordercode, int status)
		{
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == ordercode);
			if (order == null) { 
			return NotFound();
			}
			order.Status = status;
			_dataContext.Update(order);
			if (status == 0)
			{
				var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product)
					.Where(od => od.OrderCode == order.OrderCode)
					.Select(od => new
					{
						od.Quantity,
						od.Product.Price,
						od.Product.CapitalPrice
					}).ToListAsync();
				//Lấy data thống kê dựa vào ngày đặt hàng
				var statisticalModel = await _dataContext.Statisticals
					.FirstOrDefaultAsync(s => s.DateCreated.Date == order.CreateDate.Date);
				if (statisticalModel != null)
				{
					foreach(var orderDetail in DetailsOrder)
					{

						//tồn tại ngày thì cộng dồn
						statisticalModel.Quantity += 1;
						statisticalModel.Sold += orderDetail.Quantity;
                        statisticalModel.Revenue += orderDetail.Quantity + orderDetail.Price;
						statisticalModel.Profit += orderDetail.Price + orderDetail.CapitalPrice;
					}
					_dataContext.Update(statisticalModel);
				}
				else
				{
					int new_quantity = 0;
					int new_sold = 0;
					decimal new_profit = 0;
					foreach(var orderDetail in DetailsOrder)
					{
						new_quantity += 1;
						new_sold += orderDetail.Quantity;
						new_profit += orderDetail.Price - orderDetail.CapitalPrice;

						statisticalModel = new StatisticalModel
						{
							DateCreated = order.CreateDate,
							Quantity = new_quantity,
							Sold = new_sold,
							Revenue = orderDetail.Quantity + orderDetail.Price,
							Profit = new_profit
						};
					}
					_dataContext.Add(statisticalModel);
				}
				
			}
            try
            {
                await _dataContext.SaveChangesAsync();
                return Ok(new { success = true, message = "Order status update successfully" });
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while updating the order status");
            }
        }

		[Route("Delete")]
		public async Task<ActionResult> Delete(string orderCode)
		{
			// Tìm đơn hàng theo mã đơn hàng
			var order = await _dataContext.Orders.FirstOrDefaultAsync(o => o.OrderCode == orderCode);

			// Nếu không tìm thấy đơn hàng, trả về lỗi 404 (Not Found)
			if (order == null)
			{
				return NotFound();
			}

			try
			{
				// Xóa đơn hàng khỏi cơ sở dữ liệu
				_dataContext.Orders.Remove(order);

				// Lưu thay đổi vào cơ sở dữ liệu
				await _dataContext.SaveChangesAsync();

				// Trả về kết quả thành công
				TempData["success"] = "Xóa đơn hàng thành công";
				return Redirect("Index");
			}
			catch (Exception ex)
			{
				// Trả về lỗi nếu có sự cố xảy ra khi xóa
				return StatusCode(500, new { success = false, message = "Có lỗi xảy ra khi xóa đơn hàng.", error = ex.Message });
			}
		}
	}
}
