namespace Backend.Models
{
    public class PaginationQueryParameters
    {
        const int maxPageSize = 50;
        public int PageNumber { get; set; } = 1;
        private int pageSize = 25;
        public int PageSize
        {
            get { return this.pageSize; }
            set { this.pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }
    }
}