using CatalogService.Api.Core.Domain.Entities;

namespace CatalogService.Api.Services.Responses
{
    public class CatalogItemListResponse
    {
        public List<CatalogItem> CatalogItemList { get; set; }
        public long TotalCount { get; set; }
    }
}
