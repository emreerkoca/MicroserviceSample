using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Web.ApiGateway.Extensions;
using Web.ApiGateway.Models.Catalog;
using Web.ApiGateway.Services.Interfaces;

namespace Web.ApiGateway.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration configuration;

        public CatalogService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            this._httpClientFactory = httpClientFactory;
            this.configuration = configuration;
        }

        public async Task<CatalogItem> GetCatalogItemAsync(int id)
        {
            var client = _httpClientFactory.CreateClient("catalog");
            var response = await client.GetResponseAsync<CatalogItem>("items/" + id);

            return response;
        }

        public Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync(IEnumerable<int> ids)
        {
            return null;
        }
    }
}
