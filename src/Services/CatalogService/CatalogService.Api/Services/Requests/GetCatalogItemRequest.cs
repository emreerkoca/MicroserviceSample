namespace CatalogService.Api.Services.Requests
{
    public class GetCatalogItemRequest : BaseGetPaginationRequest
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? AvailableStock { get; set; }
        public bool? OnReorder { get; set; }
        public int? CatalogTypeId { get; set; }
        public int? CatalogBrandId { get; set; }
    }
}
