using CatalogServiceApi.Core.Application.ViewModels;
using CatalogServiceApi.Core.Domain.Entities;
using CatalogServiceApi.Services;
using CatalogServiceApi.Services.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace CatalogServiceApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogController : Controller
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetItemsAsync([FromQuery] GetCatalogItemRequest getCatalogItemRequest)
        {
           var catalogItemListResponse = await _catalogService.GetCatalogItemAsync(getCatalogItemRequest);
           var model = new PaginatedItemsViewModel<CatalogItem>(getCatalogItemRequest.PageIndex, getCatalogItemRequest.PageSize, catalogItemListResponse.TotalCount, catalogItemListResponse.CatalogItemList);

            return Ok(model);
        }

        [HttpGet]
        [Route("items/{id:int}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(CatalogItem), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<CatalogItem>> ItemByIdAsync(int id)
        {
            var result = await _catalogService.GetCatalogItemByIdAsync(id);

            return result;
        }

        [Route("items")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> PostProductAsync([FromBody] PostCatalogItemRequest postCatalogItemRequest)
        {
            int id = await _catalogService.PostCatalogItemAsync(postCatalogItemRequest);

            return CreatedAtAction(nameof(PostProductAsync), new { id = id }, null);
        }

        [Route("items")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] PatchCatalogItemRequest patchCatalogItemRequest)
        {
            int id = await _catalogService.UpdateCatalogItemAsync(patchCatalogItemRequest);

            return StatusCode((int)HttpStatusCode.OK, id);
        }

        [Route("{id}")]
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync([FromRoute] int id)
        {
            await _catalogService.DeleteCatalogItemAsync(id);

            return StatusCode((int)HttpStatusCode.OK);
        }
    }
}
