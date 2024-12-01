using ECommerce_web.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
		public async Task<IActionResult> ViewOrder(string ordercode)
		{
			var DetailsOrder = await _dataContext.OrderDetails.Include(od => od.Product).Where(od => od.OrderCode == ordercode).ToListAsync();
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
			try
			{
				await _dataContext.SaveChangesAsync();
				return Ok(new { success = true, message = "Order status update successfully" });
			}
			catch (Exception ex)
			{
				return StatusCode(500, " An error occurred while updating the order status");
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
