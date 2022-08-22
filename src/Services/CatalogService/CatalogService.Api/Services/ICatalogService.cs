using CatalogService.Api.Core.Domain.Entities;
using CatalogService.Api.Services.Requests;
using CatalogService.Api.Services.Responses;

namespace CatalogService.Api.Services
{
    public interface ICatalogService
    {
        Task<CatalogItemListResponse> GetCatalogItem(GetCatalogItemRequest getCatalogItemRequest);
        Task<int> PostCatalogItem(PostCatalogItemRequest postCatalogItemRequest);
        Task<int> UpdateCatalogItem(PatchCatalogItemRequest patchCatalogItemRequest);
    }
}
