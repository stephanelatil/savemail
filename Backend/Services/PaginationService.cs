using Backend.Models;
using Microsoft.EntityFrameworkCore;
namespace Backend.Services
{
    public class PaginationService
    {
        public static async Task<PagedResult<TDto>> GetPagedResult<TEntity, TDto>(
            IOrderedQueryable<TEntity> query,
            PaginationQueryParameters paginationQueryParameters,
            Func<TEntity, TDto> mapper,
            string route)
            where TEntity : class
            where TDto : class
        {
            int pageSize = paginationQueryParameters.PageSize;
            int pageNumber = paginationQueryParameters.PageNumber;

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize+1)
                .ToListAsync();

            bool hasNext = items.Count > pageSize;

            return new PagedResult<TDto>
            {
                Items = items.Take(pageSize).Select(mapper).ToList(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                Next = hasNext ? $"{route}?pageSize={pageSize}&pageNumber={pageNumber+1}": null,
                Prev = pageNumber > 1 ? $"{route}?pageSize={pageSize}&pageNumber={pageNumber-1}": null,
            };
        }
    }

    public class PagedResult<T> where T : class
    {
        public List<T> Items { get; set; } = [];
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 0;
        public string? Next { get; set; } = string.Empty;
        public string? Prev { get; set; } = string.Empty;
    }
}