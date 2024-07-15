using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Models
{
    public class PaginationQueryParameters
    {
        const int maxPageSize = 50;
        private int pageSize = 25;

        [FromQuery(Name = "pageNumber")]
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
        public int PageNumber { get; set; } = 1;

        [FromQuery(Name = "pageSize")]
        [Range(1, maxPageSize, ErrorMessage = "Page size must be between 1 and 50.")]
        public int PageSize
        {
            get { return this.pageSize; }
            set { this.pageSize = (value > maxPageSize) ? maxPageSize : value; }
        }
    }
}