using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Backend.Services
{
    public interface IPaginationService<T,U> where T : DbSet<U> where U : class
    {
        public Task<PaginatedList<U>> GetPageAsync(T querySet, string route, int page, int pageSize);
    }

    public class QueryParamTakeSkipPaginationService<T, U> : IPaginationService<T, U> where T : DbSet<U> where U : class
    {
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