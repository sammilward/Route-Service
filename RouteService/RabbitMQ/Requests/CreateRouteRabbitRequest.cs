using RouteService.Models;
using System.Collections.Generic;

namespace RouteService.RabbitMQ.Requests
{
    public class CreateRouteRabbitRequest
    {
        public string UserId { get; set; }
        public string RouteName { get; set; }
        public List<Place> Places { get; set; }
    }
}
