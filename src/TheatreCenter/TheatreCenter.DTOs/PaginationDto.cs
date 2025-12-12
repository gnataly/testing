using System.Text.Json.Serialization;

namespace TheatreCenter.DTOs;

public class PaginationDto
{
    [JsonPropertyName("page")]
    public int Page { get; set; } = 1;

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; } = 20;

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class PagedResult<T>
{
    public List<T> Items { get; set; }
    public PaginationDto Pagination { get; set; }

    public PagedResult(List<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        Pagination = new PaginationDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
