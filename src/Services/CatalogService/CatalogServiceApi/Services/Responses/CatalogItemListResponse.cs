using CatalogServiceApi.Core.Domain.Entities;
using System.Collections.Generic;

namespace CatalogServiceApi.Services.Responses
{
    public class CatalogItemListResponse
    {
        public List<CatalogItem> CatalogItemList { get; set; }
        public long TotalCount { get; set; }
    }
}
