using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Models;

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

public enum SortByEnum{
    Date,
}

public class SearchRequestDto
{
    [FromQuery(Name = "SearchTerm")]
    public string? SearchTerm { get; set; }
    
    [FromQuery(Name = "after")]
    public DateTime? FromDate { get; set; }

    [FromQuery(Name = "before")]
    public DateTime? ToDate { get; set; }
    
    [FromQuery(Name = "hasAttachments")]
    public bool? HasAttachments { get; set; }
    
    [FromQuery(Name = "isReply")]
    public bool? IsReply { get; set; }
    
    [FromQuery(Name = "hasReply")]
    public bool? HasReply { get; set; }
    
    [FromQuery(Name = "address")]
    public string? EmailAddress { get; set; }
    
    [FromQuery(Name = "sortBy")]
    public SortByEnum SortBy { get; set; } = SortByEnum.Date;
    
    [FromQuery(Name = "desc")]
    public bool SortDescending { get; set; } = true;
}
