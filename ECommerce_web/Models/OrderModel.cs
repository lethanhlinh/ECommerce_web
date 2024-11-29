namespace ECommerce_web.Models
{
    public class OrderModel
    {
        public int Id { set; get; }
        public string UserName { set; get; }
        public string OrderCode { set; get; }
        public DateTime CreateDate { set; get; }
        public int Status { set; get; }
    }
}
