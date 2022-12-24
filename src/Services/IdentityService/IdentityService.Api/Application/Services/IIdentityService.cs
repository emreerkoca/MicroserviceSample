using IdentityService.Api.Application.Services.Requests;
using IdentityService.Api.Application.Services.Responses;
using IdentityService.Application.Models;
using System.Threading.Tasks;

namespace IdentityService.Application.Services
{
    public interface IIdentityService
    {
        Task<UserResponse> PostUser(PostUserRequest postUserRequest);
        Task<LoginResponseModel> Login(LoginRequestModel requestModel);
    }
}
