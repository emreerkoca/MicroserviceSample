using IdentityService.Application.Models;
using System.Threading.Tasks;

namespace IdentityService.Application.Services
{
    public interface IIdentityService
    {
        Task<LoginResponseModel> Login(LoginRequestModel requestModel);
    }
}
