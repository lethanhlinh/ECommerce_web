namespace ECommerce_web.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; } //So luong ban
        public int Sold { get; set; } //So lượng đơn hàng
        public int Revenue { get; set; } //Doanh thu
        public int Profit { get; set; } //Lợi nhuận
        public DateTime DateCreated { get; set; } //Ngày bán
    }
}
