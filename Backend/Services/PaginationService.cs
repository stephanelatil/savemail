using Backend.Models;
using Microsoft.EntityFrameworkCore;
namespace Backend.Services
{
    public interface IPaginationService<T,U> where T : IOrderedQueryable<U> where U : class
    {
        public Task<PaginatedList<U>> GetPageAsync(T querySet, string route, int page, int pageSize);
    }

    public class QueryParamTakeSkipPaginationService<T, U> : IPaginationService<T, U> where T : IOrderedQueryable<U> where U : class
    {
        /// <summary>
        /// Returns a PaginatedList gotten from the given queryset. 
        /// </summary>
        /// <param name="querySet">The queryset to paginate. NOTE: it should already be ordered-by whatever key you require.</param>
        /// <param name="route">The query URL (without query params)</param>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The max number of element of the list</param>
        /// <returns>A PaginatedList object</returns>
        public async Task<PaginatedList<U>> GetPageAsync(T querySet, string route, int page, int pageSize)
        {
            // gets one extra element just to make sure that there are more after
            var data = await querySet
                .Skip((page - 1) * pageSize)
                .Take(pageSize+1)
                .ToListAsync();
            // hasNext checks if we managed to get one element after the last requested
            bool hasNext = data.Count > pageSize;
            //Remove extra element
            if (hasNext) data.RemoveAt(pageSize);

            return new PaginatedList<U>(data, page, route, hasNext);
        }
    }
}