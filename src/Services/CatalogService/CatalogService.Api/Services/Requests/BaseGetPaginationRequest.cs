namespace CatalogService.Api.Services.Requests
{
    public class BaseGetPaginationRequest
    {
        public int PageSize { get; set; } = 20;
        public int PageIndex { get; set; } = 0;
    }
}
