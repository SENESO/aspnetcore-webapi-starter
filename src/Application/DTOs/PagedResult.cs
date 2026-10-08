namespace Application.DTOs;

// Standard paged response wrapper: the items for the current page plus
// metadata so clients can build pagination UI without extra calls.
public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// Bound from the query string: GET /api/products/paged?page=2&pageSize=10
// Values are clamped so nonsense input can never break the query.
public record PaginationParams(int Page = 1, int PageSize = 20)
{
    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize is < 1 or > 100 ? 20 : PageSize;
}
