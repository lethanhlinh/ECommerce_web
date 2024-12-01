namespace ECommerce_web.Models
{
    public class Paginate
    {
        public int TotalItems { get; private set; } //Tổng số items
        public int PageSize { get; private set; } // Tổng số item/trang
        public int CurrentPage {get; private set; } //Trang hiện tại
        
        public int TotalPages { get; private set; } //Tổng số trang
        public int StartPage { get; private set; } //Trang bắt đầu
        public int EndPage { get; private set; } //Trang kết thúc

        public Paginate()
        {

        }
        public Paginate(int totalItems, int page, int pageSize = 10) //10 Items trên trang
        {
            //làm tròn tổng items/10 trên 1 trang vd 16 items/10 tròn 2 trang
            int totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)pageSize); // 33/10 =3.3 => 4 trang

            int currentPage = page; //page hiện tại =1

            int startPage = currentPage - 5; //trang bắt đầu trừ 5 button
            int endPage = currentPage + 4; //Trang cuối sẽ cộng 4 button

            if(startPage <= 0)
            {
                //nếu số trang bắt đầu nhỏ hơn hoặc bằng 0 thì số trang cuối sẽ bằng:
                endPage = endPage - (startPage - 1); // 6-(-3-1)=10
                startPage = 1;
            }
            if (endPage > totalPages) // nếu số page cuối > số tổng trang
            {
                endPage = totalPages; // số page cuối = tổng page
                if(endPage > 10) // nếu số page cuối >10
                {
                    startPage = endPage - 9; //trang bắt đầu bằng trang cuối - 9
                }    
            }
            TotalItems = totalItems;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalPages = totalPages;    
            StartPage = startPage;
            EndPage = endPage;
        }
    }
}
