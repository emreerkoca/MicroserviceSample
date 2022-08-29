using CatalogService.Api.Core.Domain.Entities;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using CatalogService.Api.Services.Requests;
using CatalogService.Api.Services.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CatalogService.Api.Services
{
    public class CatalogServiceImplementation : ICatalogService
    {
        private readonly CatalogContext _catalogContext;
        private readonly CatalogSettings _catalogSettings;

        public CatalogServiceImplementation(CatalogContext catalogContext, IOptions<CatalogSettings> catalogSettings)
        {
            _catalogContext = catalogContext ?? throw new ArgumentNullException(nameof(catalogContext));
            _catalogSettings = catalogSettings.Value;

            catalogContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        public async Task<CatalogItemListResponse> GetCatalogItemAsync(GetCatalogItemRequest getCatalogItemRequest)
        {
            IQueryable<CatalogItem> catalogItemQueryable = _catalogContext.CatalogItems.AsQueryable();

            if (getCatalogItemRequest.Id.HasValue)
            {
                catalogItemQueryable = catalogItemQueryable.Where(m => m.Id == getCatalogItemRequest.Id);
            }

            if (!string.IsNullOrEmpty(getCatalogItemRequest.Name))
            {
                catalogItemQueryable = catalogItemQueryable.Where(m => m.Name == getCatalogItemRequest.Name);
            }

            if (getCatalogItemRequest.AvailableStock.HasValue)
            {
                catalogItemQueryable = catalogItemQueryable.Where(m => m.AvailableStock == getCatalogItemRequest.AvailableStock);
            }

            if (getCatalogItemRequest.OnReorder.HasValue)
            {
                catalogItemQueryable = catalogItemQueryable.Where(m => m.OnReorder == getCatalogItemRequest.OnReorder);
            }

            if (getCatalogItemRequest.CatalogTypeId.HasValue)
            {
                catalogItemQueryable = catalogItemQueryable.Where(m => m.CatalogTypeId == getCatalogItemRequest.CatalogTypeId);
            }

            if (getCatalogItemRequest.CatalogBrandId.HasValue)
            {
                catalogItemQueryable = catalogItemQueryable.Where(m => m.CatalogBrandId == getCatalogItemRequest.CatalogBrandId);
            }

            var totalItemCount = await _catalogContext.CatalogItems.LongCountAsync();
            var catalogItemList = await _catalogContext.CatalogItems
                .OrderBy(c => c.Name)
                .Skip(getCatalogItemRequest.PageSize * getCatalogItemRequest.PageIndex)
                .Take(getCatalogItemRequest.PageSize)
                .ToListAsync();

            catalogItemList = ChangeUriPlaceholder(catalogItemList);

            return new CatalogItemListResponse
            {
                CatalogItemList = catalogItemList,
                TotalCount = totalItemCount
            };
        }

        public async Task<int> PostCatalogItemAsync(PostCatalogItemRequest postCatalogItemRequest)
        {
            var catalogItem = new CatalogItem
            {
                CatalogBrandId = postCatalogItemRequest.CatalogBrandId,
                CatalogTypeId = postCatalogItemRequest.CatalogTypeId,
                Description = postCatalogItemRequest.Description,
                Name = postCatalogItemRequest.Name,
                PictureFileName = postCatalogItemRequest.PictureFileName,
                Price = postCatalogItemRequest.Price
            };

            _catalogContext.CatalogItems.Add(catalogItem);

            await _catalogContext.SaveChangesAsync();

            return catalogItem.Id;
        }

        public async Task<int> UpdateCatalogItemAsync(PatchCatalogItemRequest patchCatalogItemRequest)
        {
            var catalogItem = await _catalogContext.CatalogItems.SingleOrDefaultAsync(i => i.Id == patchCatalogItemRequest.Id);

            if (catalogItem == null)
            {
                throw new Exception($"Item with id {patchCatalogItemRequest.Id} not found.");
            }

            var oldPrice = catalogItem.Price;
            var isCatalogItemPriceChanged = oldPrice != patchCatalogItemRequest.Price;

            if (patchCatalogItemRequest.CatalogTypeId.HasValue)
            {
                catalogItem.CatalogTypeId = patchCatalogItemRequest.CatalogTypeId.Value;
            }

            if (patchCatalogItemRequest.AvailableStock.HasValue)
            {
                catalogItem.AvailableStock = patchCatalogItemRequest.AvailableStock.Value;
            }

            if (patchCatalogItemRequest.OnReorder.HasValue)
            {
                catalogItem.OnReorder = patchCatalogItemRequest.OnReorder.Value;
            }

            if (patchCatalogItemRequest.CatalogBrandId.HasValue)
            {
                catalogItem.CatalogBrandId = patchCatalogItemRequest.CatalogBrandId.Value;
            }

            if (!string.IsNullOrEmpty(patchCatalogItemRequest.Description))
            {
                catalogItem.Description = patchCatalogItemRequest.Description;
            }
            

            if (!string.IsNullOrEmpty(patchCatalogItemRequest.PictureFileName))
            {
                catalogItem.PictureFileName = patchCatalogItemRequest.PictureFileName;
            }

            if (!string.IsNullOrEmpty(patchCatalogItemRequest.Name))
            {
                catalogItem.Name = patchCatalogItemRequest.Name;
            }

            if (!string.IsNullOrEmpty(patchCatalogItemRequest.PictureUri))
            {
                catalogItem.PictureUri = patchCatalogItemRequest.PictureUri;
            }

            if (patchCatalogItemRequest.Price.HasValue)
            {
                catalogItem.Price = patchCatalogItemRequest.Price.Value;
            }

            _catalogContext.CatalogItems.Update(catalogItem);

            await _catalogContext.SaveChangesAsync();

            //if (isCatalogItemPriceChanged) // Save product's data and publish integration event through the Event Bus if price has changed
            //{
            //    //Create Integration Event to be published through the Event Bus
            //    var priceChangedEvent = new ProductPriceChangedIntegrationEvent(catalogItem.Id, productToUpdate.Price, oldPrice);

            //    // Achieving atomicity between original Catalog database operation and the IntegrationEventLog thanks to a local transaction
            //    await _catalogIntegrationEventService.SaveEventAndCatalogContextChangesAsync(priceChangedEvent);

            //    // Publish through the Event Bus and mark the saved event as published
            //    await _catalogIntegrationEventService.PublishThroughEventBusAsync(priceChangedEvent);
            //}

            return catalogItem.Id;
        }

        private List<CatalogItem> ChangeUriPlaceholder(List<CatalogItem> items)
        {
            var baseUri = _catalogSettings.PicBaseUrl;

            foreach (var item in items)
            {
                if (item != null)
                {
                    item.PictureUri = baseUri + item.PictureFileName;
                }
            }

            return items;
        }

        public async Task DeleteCatalogItemAsync(int id)
        {
            var catalogItem = _catalogContext.CatalogItems.SingleOrDefault(x => x.Id == id);

            if (catalogItem == null)
            {
                throw new Exception($"Item couldn't found with id: {id}");
            }

            _catalogContext.CatalogItems.Remove(catalogItem);

            await _catalogContext.SaveChangesAsync();
        }
    }
}
