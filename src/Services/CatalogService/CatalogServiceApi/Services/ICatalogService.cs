using CatalogServiceApi.Services.Requests;
using CatalogServiceApi.Services.Responses;
using System.Threading.Tasks;

namespace CatalogServiceApi.Services
{
    public interface ICatalogService
    {
        Task<CatalogItemListResponse> GetCatalogItemAsync(GetCatalogItemRequest getCatalogItemRequest);
        Task<int> PostCatalogItemAsync(PostCatalogItemRequest postCatalogItemRequest);
        Task<int> UpdateCatalogItemAsync(PatchCatalogItemRequest patchCatalogItemRequest);
        Task DeleteCatalogItemAsync(int id);
    }
}
