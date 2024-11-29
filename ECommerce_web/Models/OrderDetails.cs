namespace ECommerce_web.Models
{
    public class OrderDetails
    {
        public int Id { set; get; }
        public string UserName { set; get; }
        public string OrderCode { set; get; }

        public int ProductId { set; get; }
        public decimal Price { set; get; }
        public int Quantity { set; get; }
    }
}
