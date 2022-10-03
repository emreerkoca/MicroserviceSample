using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using WebApp.Extensions;
using WebApp.Infrastructure;

namespace WebApp.Utils
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly HttpClient client;
        private readonly AuthenticationState anonymous;
        private readonly AppStateManager appState;


        public AuthStateProvider(ILocalStorageService LocalStorageService, HttpClient Client, AppStateManager appState)
        {
            _localStorageService = LocalStorageService;
            client = Client;
            anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            this.appState = appState;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            string apiToken = await _localStorageService.GetToken();

            if (String.IsNullOrEmpty(apiToken))
                return anonymous;

            string userName = await _localStorageService.GetUsername();

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)
            }, "jwtAuthType"));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiToken);

            return new AuthenticationState(claimsPrincipal);
        }

        public void NotifyUserLogin(String userName)
        {
            var cp = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName)

            }, "jwtAuthType"));

            var authState = Task.FromResult(new AuthenticationState(cp));

            NotifyAuthenticationStateChanged(authState);
            appState.LoginChanged(null);
        }

        public void NotifyUserLogout()
        {
            var authState = Task.FromResult(anonymous);
            NotifyAuthenticationStateChanged(authState);
            appState.LoginChanged(null);
        }
    }
}
