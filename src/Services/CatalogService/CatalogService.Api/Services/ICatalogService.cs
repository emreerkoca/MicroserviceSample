﻿using CatalogService.Api.Services.Requests;
using CatalogService.Api.Services.Responses;

namespace CatalogService.Api.Services
{
    public interface ICatalogService
    {
        Task<CatalogItemListResponse> GetCatalogItemAsync(GetCatalogItemRequest getCatalogItemRequest);
        Task<int> PostCatalogItemAsync(PostCatalogItemRequest postCatalogItemRequest);
        Task<int> UpdateCatalogItemAsync(PatchCatalogItemRequest patchCatalogItemRequest);
        Task DeleteCatalogItemAsync(int id);
    }
}
