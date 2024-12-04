using System.ComponentModel.DataAnnotations;

namespace ECommerce_web.Models
{
	public class CouponModel
	{
		[Key]
		public int Id { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập tên Mã giảm giá")]
		public string Name { get; set; }

		[Required(ErrorMessage = "Yêu cầu nhập mô tả mã giảm giá")]
		public string Description { get; set; }

		public DateTime DateStart { get; set; }
		public DateTime DateExpired { get; set; }

	

		[Required(ErrorMessage = "Yêu cầu nhập số lượng mã giảm giá")]
		public string Quantity { get; set; }

		public int Status { get; set; }
	}
}
