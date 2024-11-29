using System.ComponentModel.DataAnnotations;

namespace ECommerce_web.Models.ViewModels
{
	public class LoginViewModel
	{
		public int Id { get; set; }
		[Required(ErrorMessage = "Làm ơn nhập Username")]
		public string Username { get; set; }
		[DataType(DataType.Password), Required(ErrorMessage = "Làm ơn nhập Password")]
		public string Password { get; set; }
		public string ReturnURL { get; set; }
	}
}
