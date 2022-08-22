using CatalogService.Api.Core.Application.ViewModels;
using CatalogService.Api.Core.Domain.Entities;
using CatalogService.Api.Infrastructure;
using CatalogService.Api.Infrastructure.Context;
using CatalogService.Api.Services;
using CatalogService.Api.Services.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net;

namespace CatalogService.Api.Controllers
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Route("items")]
        [ProducesResponseType(typeof(PaginatedItemsViewModel<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(IEnumerable<CatalogItem>), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetItemsAsync([FromQuery] GetCatalogItemRequest getCatalogItemRequest)
        {
           var catalogItemListResponse = await _catalogService.GetCatalogItem(getCatalogItemRequest);
           var model = new PaginatedItemsViewModel<CatalogItem>(getCatalogItemRequest.PageIndex, getCatalogItemRequest.PageSize, catalogItemListResponse.TotalCount, catalogItemListResponse.CatalogItemList);

            return Ok(model);
        }

        [Route("items")]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> PostProductAsync([FromBody] PostCatalogItemRequest postCatalogItemRequest)
        {
            int id = await _catalogService.PostCatalogItem(postCatalogItemRequest);

            return CreatedAtAction(nameof(PostProductAsync), new { id = id }, null);
        }

        [Route("items")]
        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> UpdateProductAsync([FromBody] PatchCatalogItemRequest patchCatalogItemRequest)
        {
            int id = await _catalogService.UpdateCatalogItem(patchCatalogItemRequest);

            return StatusCode((int)HttpStatusCode.OK, id);
        }

        [Route("{id}")]
        [HttpDelete]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult> DeleteProductAsync(int id)
        {
            return NoContent();
        }
    }
}
