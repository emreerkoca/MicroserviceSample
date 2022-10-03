using System.Net.Http;
using System.Threading.Tasks;
using WebApp.Application.Services.Interfaces;
using WebApp.Domain.Models;
using WebApp.Domain.Models.CatalogModels;
using WebApp.Extensions;

namespace WebApp.Application.Services
{
    public class CatalogService : ICatalogService
    {
        private readonly HttpClient httpClient;

        public CatalogService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<PaginatedItemsViewModel<CatalogItem>> GetCatalogItems()
        {
            var response = await httpClient.GetResponseAsync<PaginatedItemsViewModel<CatalogItem>>("/catalog/items");

            return response;
        }
    }
}
