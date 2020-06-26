using RouteService.Models;

namespace RouteService.RabbitMQ.Responses
{
    public class GetRouteRabbitResponse
    {
        public bool FoundRoute { get; set; }
        public Route Route { get; set; }
    }
}
