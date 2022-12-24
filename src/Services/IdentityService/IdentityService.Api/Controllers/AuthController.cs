using IdentityService.Api.Application.Services.Requests;
using IdentityService.Application.Models;
using IdentityService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IdentityService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel loginRequestModel)
        {
            var result = await _identityService.Login(loginRequestModel);

            return Ok(result);
        }

        [HttpPost("user")]
        public async Task<IActionResult> PostUser([FromBody] PostUserRequest postUserRequest)
        {
            var result = await _identityService.PostUser(postUserRequest);

            return Ok(result);
        }
    }
}
