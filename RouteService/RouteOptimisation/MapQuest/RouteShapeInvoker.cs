using Microsoft.Extensions.Configuration;
using RouteService.HTTP;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteService.RouteOptimisation.MapQuest
{
    public class RouteShapeInvoker : IRouteShapeInvoker
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientWrapper _httpClientWrapper;

        private const string requestUrl = "https://www.mapquestapi.com/directions/v2/routeshape";

        public RouteShapeInvoker(IConfiguration configuration, IHttpClientWrapper httpClientWrapper)
        {
            _configuration = configuration;
            _httpClientWrapper = httpClientWrapper;
        }

        public async Task<RouteShapeResponse> MakeOptimisedRouteRequestAndWaitAsync(OptimisedRouteResponse optimisedRouteResponse)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("key", _configuration.GetSection("MapQuestKey").Value),
                new KeyValuePair<string, string>("fullShape", "true"),
                new KeyValuePair<string, string>("sessionId", optimisedRouteResponse.route.sessionId)
            };

            return await _httpClientWrapper.GetAsync<RouteShapeResponse>(requestUrl, parameters);
        }
    }
}
