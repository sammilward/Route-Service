using Microsoft.Extensions.Configuration;
using RouteService.HTTP;
using RouteService.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RouteService.RouteOptimisation.MapQuest
{
    public class OptimiseRouteInvoker : IOptimiseRouteInvoker
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientWrapper _httpClientWrapper;

        private const string requestUrl = "https://www.mapquestapi.com/directions/v2/optimizedRoute";

        public OptimiseRouteInvoker(IConfiguration configuration, IHttpClientWrapper httpClientWrapper)
        {
            _configuration = configuration;
            _httpClientWrapper = httpClientWrapper;
        }

        public async Task<OptimisedRouteResponse> MakeOptimisedRouteRequestAndWaitAsync(Route route, User user)
        {
            List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
            parameters.Add(new KeyValuePair<string, string>("key", _configuration.GetSection("MapQuestKey").Value));
            parameters.Add(new KeyValuePair<string, string>("outFormat", "json"));

            var request = new OptimisedRouteRequest() { locations = new List<string>() };
            request.locations.Add($"{user.Latitude},{user.Longitude}");
            route.Places.ForEach(x => request.locations.Add($"{x.Latitude},{x.Longitude}"));

            return await _httpClientWrapper.PostAsync<OptimisedRouteResponse>(requestUrl, request, parameters);
        }
    }
}
