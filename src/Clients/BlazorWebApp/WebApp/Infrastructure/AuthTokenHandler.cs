using Blazored.LocalStorage;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using WebApp.Extensions;

namespace WebApp.Infrastructure
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly ISyncLocalStorageService _syncLocalStorageService;

        public AuthTokenHandler(ISyncLocalStorageService syncLocalStorageService)
        {
            _syncLocalStorageService = syncLocalStorageService;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_syncLocalStorageService != null)
            {
                string token = _syncLocalStorageService.GetToken();
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", token);
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
