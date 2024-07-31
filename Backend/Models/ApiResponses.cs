namespace Backend.Models
{
    public class PaginatedList<T>
    {
        public List<T> Items { get; }
        public int PageIndex { get; }
        public string? PreviousPage { get; }
        public string? NextPage { get; }

        public PaginatedList(List<T> items, int pageIndex, string route, bool hasNext = false)
        {
            this.Items = items;
            this.PageIndex = pageIndex;
            this.PreviousPage = pageIndex >= 1 ? $"{route}?page={pageIndex-1}" : null;
            this.NextPage = hasNext ? null : $"{route}?page={pageIndex+1}";
        }
    }
}