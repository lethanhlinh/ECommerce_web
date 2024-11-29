using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerce_web.Models
{
    public class OrderDetails
    {
        public int Id { set; get; }
        public string UserName { set; get; }
        public string OrderCode { set; get; }

        public long ProductId { set; get; }
        public decimal Price { set; get; }
        public int Quantity { set; get; }

        [ForeignKey("ProductId")]
        public ProductModel Product { set; get; }
    }
}
