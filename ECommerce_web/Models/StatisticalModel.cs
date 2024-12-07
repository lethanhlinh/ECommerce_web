namespace ECommerce_web.Models
{
    public class StatisticalModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; } //So luong ban
        public int Sold { get; set; } //So lượng đơn hàng
        public decimal Revenue { get; set; } //Doanh thu
        public decimal Profit { get; set; } //Lợi nhuận
        public DateTime DateCreated { get; set; } //Ngày bán
    }
}
