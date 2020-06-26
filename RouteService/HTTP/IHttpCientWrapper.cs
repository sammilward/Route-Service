using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteService.HTTP
{
    public interface IHttpClientWrapper
    {
        public Task<T> GetAsync<T>(string requestUrl);
        Task<T> GetAsync<T>(string requestUrl, List<KeyValuePair<string, string>> parameters);
        Task<T> PostAsync<T>(string requestUri, Object body, List<KeyValuePair<string, string>> parameters);
    }
}
