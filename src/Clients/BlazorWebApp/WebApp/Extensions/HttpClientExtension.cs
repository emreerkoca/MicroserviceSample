using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace WebApp.Extensions
{
    public static class HttpClientExtension
    {
        public async static Task<TResult> PostGetResponseAsync<TResult, TValue>(this HttpClient httpClient, string Url, TValue Value)
        {
            var httpResponse = await httpClient.PostAsJsonAsync(Url, Value);

            return httpResponse.IsSuccessStatusCode ? await httpResponse.Content.ReadFromJsonAsync<TResult>() : default;
        }

        public async static Task PostAsync<TValue>(this HttpClient httpClient, string Url, TValue Value)
        {
            await httpClient.PostAsJsonAsync(Url, Value);
        }


        public async static Task<T> GetResponseAsync<T>(this HttpClient httpClient, string Url)
        {
            return await httpClient.GetFromJsonAsync<T>(Url);
        }
    }
}
