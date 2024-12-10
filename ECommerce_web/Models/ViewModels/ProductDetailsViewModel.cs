using System.ComponentModel.DataAnnotations;

namespace ECommerce_web.Models.ViewModels
{
	public class ProductDetailsViewModel
	{
		public ProductModel ProductDetails { get; set; }
		[Required(ErrorMessage ="Yêu cầu nhập bình luận sản phẩm")]		
		public string Comment { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập tên")]
		public string Name { get; set; }
		[Required(ErrorMessage = "Yêu cầu nhập Email")]
		public string Email { get; set; }

		// Thêm danh sách chi tiết sản phẩm
		public RatingModel Rating { get; set; }
	}
}
