using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RouteService.HTTP
{
    public class HttpClientWrapper : IHttpClientWrapper
    {
        private readonly HttpClient _httpClient;

        public HttpClientWrapper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<T> GetAsync<T>(string requestUri, List<KeyValuePair<string, string>> parameters)
        {
            if (parameters != null && parameters.Any())
            {
                requestUri += "?";
                parameters.ForEach(x => requestUri += $"&{x.Key}={x.Value}");   
            }

            return GetAsync<T>(requestUri);
        }

        public async Task<T> GetAsync<T>(string requestUri)
        {
            var response = await _httpClient.GetAsync(requestUri);

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }

        public async Task<T> PostAsync<T> (string requestUri, Object body, List<KeyValuePair<string, string>> parameters)
        {
            if (parameters != null && parameters.Any())
            {
                requestUri += "?";
                parameters.ForEach(x => requestUri += $"&{x.Key}={x.Value}");
            }

            var stringContent = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(requestUri, stringContent);

            return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
        }
    }
}
